// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;

namespace WinUIGallery.Controls;

public enum ColorTileBackdropKind
{
    None,
    Acrylic,
    Mica,
    MicaAlt,
}

public sealed partial class ColorTile : UserControl
{
    private CanvasImageBrush? checkerBrush;
    private CanvasRenderTarget? checkerboardTile;

    public string ColorName
    {
        get { return (string)GetValue(ColorNameProperty); }
        set { SetValue(ColorNameProperty, value); }
    }
    public static readonly DependencyProperty ColorNameProperty =
        DependencyProperty.Register("ColorName", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

    public string ColorExplanation
    {
        get { return (string)GetValue(ColorExplanationProperty); }
        set { SetValue(ColorExplanationProperty, value); }
    }
    public static readonly DependencyProperty ColorExplanationProperty =
        DependencyProperty.Register("ColorExplanation", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

    public string ColorBrushName
    {
        get { return (string)GetValue(ColorBrushNameProperty); }
        set { SetValue(ColorBrushNameProperty, value); }
    }
    public static readonly DependencyProperty ColorBrushNameProperty =
        DependencyProperty.Register("ColorBrushName", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

    public string ColorValue
    {
        get { return (string)GetValue(ColorValueProperty); }
        set { SetValue(ColorValueProperty, value); }
    }
    public static readonly DependencyProperty ColorValueProperty =
        DependencyProperty.Register("ColorValue", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

    public bool ShowSeparator
    {
        get { return (bool)GetValue(ShowSeparatorProperty); }
        set { SetValue(ShowSeparatorProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ShowSeparator.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ShowSeparatorProperty =
        DependencyProperty.Register("ShowSeparator", typeof(bool), typeof(ColorTile), new PropertyMetadata(true));

    public object Comment
    {
        get { return GetValue(CommentProperty); }
        set { SetValue(CommentProperty, value); }
    }
    public static readonly DependencyProperty CommentProperty =
        DependencyProperty.Register(
            "Comment",
            typeof(object),
            typeof(ColorTile),
            new PropertyMetadata(null, OnCommentChanged));

    private static void OnCommentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ColorTile tile = (ColorTile)d;
        tile.CommentHost.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public ColorTileBackdropKind Backdrop
    {
        get { return (ColorTileBackdropKind)GetValue(BackdropProperty); }
        set { SetValue(BackdropProperty, value); }
    }
    public static readonly DependencyProperty BackdropProperty =
        DependencyProperty.Register(
            "Backdrop",
            typeof(ColorTileBackdropKind),
            typeof(ColorTile),
            new PropertyMetadata(ColorTileBackdropKind.None, OnBackdropChanged));

    private static void OnBackdropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ColorTile)d).ApplyBackdrop();
    }

    private void ApplyBackdrop()
    {
        switch (Backdrop)
        {
            case ColorTileBackdropKind.Acrylic:
                BackdropHost.SystemBackdrop = new DesktopAcrylicBackdrop();
                BackdropHost.Visibility = Visibility.Visible;
                break;
            case ColorTileBackdropKind.Mica:
                BackdropHost.SystemBackdrop = new MicaBackdrop { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base };
                BackdropHost.Visibility = Visibility.Visible;
                break;
            case ColorTileBackdropKind.MicaAlt:
                BackdropHost.SystemBackdrop = new MicaBackdrop { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
                BackdropHost.Visibility = Visibility.Visible;
                break;
            default:
                BackdropHost.SystemBackdrop = null;
                BackdropHost.Visibility = Visibility.Collapsed;
                break;
        }
    }

    public ColorTile()
    {
        InitializeComponent();
        Unloaded += ColorTile_Unloaded;
    }

    private void CheckerboardCanvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        checkerBrush?.Dispose();
        checkerboardTile?.Dispose();

        int checkerSize = 8;
        int tileSize = checkerSize * 2;

        checkerboardTile = new CanvasRenderTarget(sender, tileSize, tileSize, 96);

        using (CanvasDrawingSession drawingSession = checkerboardTile.CreateDrawingSession())
        {
            Color lightColor = Colors.White;
            Color darkColor = Colors.Black;

            drawingSession.Clear(lightColor);
            drawingSession.FillRectangle(0, 0, checkerSize, checkerSize, darkColor);
            drawingSession.FillRectangle(checkerSize, checkerSize, checkerSize, checkerSize, darkColor);
        }

        checkerBrush = new CanvasImageBrush(sender, checkerboardTile)
        {
            ExtendX = CanvasEdgeBehavior.Wrap,
            ExtendY = CanvasEdgeBehavior.Wrap,
        };
    }

    private void CheckerboardCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (checkerBrush is null)
        {
            return;
        }

        Rect bounds = new Rect(0, 0, sender.ActualWidth, sender.ActualHeight);
        args.DrawingSession.FillRectangle(bounds, checkerBrush);
    }

    private void ColorTile_Unloaded(object sender, RoutedEventArgs e)
    {
        checkerBrush?.Dispose();
        checkerBrush = null;

        checkerboardTile?.Dispose();
        checkerboardTile = null;
    }

    private void CopyBrushNameButton_Click(object sender, RoutedEventArgs e)
    {
        DataPackage package = new DataPackage();
        package.SetText(ColorBrushName);
        Clipboard.SetContent(package);
    }
}
