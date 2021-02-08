﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using LiveShot.API;
using LiveShot.API.Events.Input;
using LiveShot.API.Image;
using LiveShot.UI.Controls.Button;
using LiveShot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace LiveShot.UI.Views
{
    public partial class CaptureScreenView
    {
        private readonly IEventPipeline _events;
        private readonly IServiceProvider _services;

        private ExportWindowView? _exportWindow;
        private Bitmap? _screenShot;

        public CaptureScreenView(IEventPipeline events, IServiceProvider services)
        {
            InitializeComponent();

            _events = events;
            _services = services;

            Top = SystemParameters.VirtualScreenTop;
            Left = SystemParameters.VirtualScreenLeft;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            SelectCanvas.Width = Width;
            SelectCanvas.Height = Height;
            SelectCanvas.WithEvents(events);

            CanvasRightPanel.With(events, Width, Height);
            CanvasBottomPanel.With(events, Width, Height);

            var actionButtons = WindowUtils.FindVisualChildren<ActionButton>(SelectCanvas).ToList();

            foreach (var actionButton in actionButtons)
                actionButton.Click += (actual, _) => ActionButtonOnClick(actual, actionButtons);

            ColorPickerBtn.Click += ColorPickerBtnOnClick;

            UploadBtn.Click += UploadBtnOnClick;
            GoogleBtn.Click += GoogleBtnOnClick;
            CopyBtn.Click += CopyBtnOnClick;
            SaveBtn.Click += SaveBtnOnClick;
            CloseBtn.Click += CloseBtnOnClick;

            CaptureScreen();
        }

        private static bool IsCtrlPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        private void ColorPickerBtnOnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var color = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);

            SelectCanvas.Color = new SolidColorBrush(color);
        }

        private static void ActionButtonOnClick(object sender, IEnumerable<ActionButton> all)
        {
            foreach (var button in all)
            {
                if (button == sender)
                {
                    button.IsActive = !button.IsActive;
                    button.UpdateAction();

                    continue;
                }

                button.IsActive = false;
            }
        }

        private void GoogleBtnOnClick(object sender, RoutedEventArgs e)
        {
            OpenExportWindow(true);
        }

        private void CloseBtnOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveBtnOnClick(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        private void CopyBtnOnClick(object sender, RoutedEventArgs e)
        {
            CopyImage();
        }

        private void UploadBtnOnClick(object sender, RoutedEventArgs e)
        {
            OpenExportWindow();
        }

        private void CaptureScreen()
        {
            (int screenTop, int screenLeft, int screenWidth, int screenHeight) =
                ((int, int, int, int)) (Top, Left, Width, Height);

            var bitmap = ImageUtils.CaptureScreen(screenWidth, screenHeight, screenLeft, screenTop);
            var bitmapSource = ImageUtils.GetBitmapSource(bitmap);

            _screenShot = bitmap;

            Background = Brushes.Transparent;
            SelectCanvas.Background = new ImageBrush(bitmapSource);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.C:
                    if (IsCtrlPressed)
                        CopyImage();
                    break;
                case Key.S:
                    if (IsCtrlPressed)
                        SaveImage();
                    break;
                case Key.D:
                    if (IsCtrlPressed)
                        OpenExportWindow();
                    break;
            }

            _events.Dispatch<OnKeyDown>(e);
        }

        private void SaveImage()
        {
            if (_screenShot is null) return;

            ImageSaveFormat[]? formats = ImageSaveFormats.Supported;

            SaveFileDialog dialog = new()
            {
                Filter = string.Join('|', formats.Select(f => f.Filter)),
                FileName = string.Format(API.Properties.Resources.CaptureScreen_SaveImage_FileName, DateTime.Now),
                RestoreDirectory = true,
                Title = API.Properties.Resources.CaptureScreen_SaveImage_Title
            };

            if (dialog.ShowDialog() == false) return;

            if (string.IsNullOrWhiteSpace(dialog.FileName)) return;

            try
            {
                FileStream fs = (FileStream) dialog.OpenFile();

                var selectedFormat = formats[dialog.FilterIndex - 1];

                var selection = SelectCanvas.Selection;
                if (selection is null || selection.IsClear) return;

                ImageUtils.GetBitmap(selection, _screenShot, SelectCanvas).Save(fs, selectedFormat.Format);

                fs.Close();
            }
            catch
            {
                return;
            }

            Close();
        }

        private void OpenExportWindow(bool google = false)
        {
            if (_exportWindow is not null || _screenShot is null) return;

            var selection = SelectCanvas.Selection;

            if (selection is null || selection.IsClear || selection.HasInvalidSize) return;

            var bitmap = ImageUtils.GetBitmap(selection, _screenShot, SelectCanvas);

            _exportWindow = _services.GetService<ExportWindowView>();

            if (_exportWindow is null) return;

            _exportWindow.Show();

            double x = Width - _exportWindow.Width - 100;
            double y = Height - _exportWindow.Height - 100;

            _exportWindow.Left = x;
            _exportWindow.Top = y;

            _exportWindow.Upload(bitmap, google);

            Close();
        }

        private void CopyImage()
        {
            var selection = SelectCanvas.Selection;
            if (selection is null || selection.IsClear) return;

            bool copied = ImageUtils.CopyImage(selection, _screenShot, SelectCanvas);

            if (copied) Close();
        }
    }
}