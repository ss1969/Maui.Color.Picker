using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Color = Microsoft.Maui.Graphics.Color;
using Point = Microsoft.Maui.Graphics.Point;
using RectF = Microsoft.Maui.Graphics.RectF;

/*
 Range:
    H : 0~359
    S : 0~100
    V : 0~100
    R : 0~255
    G : 0~255
    B : 0~255
    A : 0~255

 TODO: 
    speed optimize (use dirty rect ?)
    parameter change cannot correctly draw 
    add color name float label
    2 Color Picker will interfere
 */
namespace Controls;

public static class ColorExtensions
{
    public static System.Drawing.Color ToDrawingColor( this Microsoft.Maui.Graphics.Color mauiColor )
    {
        return System.Drawing.Color.FromArgb(
            (int)( mauiColor.Alpha * 255 ),
            (int)( mauiColor.Red * 255 ),
            (int)( mauiColor.Green * 255 ),
            (int)( mauiColor.Blue * 255 ) );
    }
    public static Microsoft.Maui.Graphics.Color ToMauiColor( this System.Drawing.Color drawingColor )
    {
        return new Microsoft.Maui.Graphics.Color(
            drawingColor.R / 255f,
            drawingColor.G / 255f,
            drawingColor.B / 255f,
            drawingColor.A / 255f );
    }
}

public class ColorDataLinked : INotifyPropertyChanged
{
    // Only 1 notify for all related properties update to UI, no separate properties,
    // that will cause unhandlable loop notification of set.
    // use basic property syntax for value clamp, value sync, fastly update.

    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged( string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    // Events
    public event Action<string?>? ParameterChanged = null;

    // Properties
    public Color _PickedColor;
    public Color PickedColor
    {
        get => _PickedColor;
        set
        {
            if ( _PickedColor != value )
            {
                _PickedColor = value;
                SyncAndNotify();
            }
        }
    }

    public string _PickedColorHEX;
    public string PickedColorHEX
    {
        get => _PickedColorHEX;
        set
        {
            if ( _PickedColorHEX != value )
            {
                _PickedColorHEX = value;
                if ( _PickedColorHEX.Length == 7 ) SyncAndNotify();
            }
        }
    }

    public byte _Red;
    public byte Red
    {
        get => _Red;
        set
        {
            if ( _Red != value )
            {
                _Red = value;
                SyncAndNotify();
            }
        }
    }

    public byte _Green;
    public byte Green
    {
        get => _Green;
        set
        {
            if ( _Green != value )
            {
                _Green = value;
                SyncAndNotify();
            }
        }
    }

    public byte _Blue;
    public byte Blue
    {
        get => _Blue;
        set
        {
            if ( _Blue != value )
            {
                _Blue = value;
                SyncAndNotify();
            }
        }
    }

    public int _Hue;
    public int Hue
    {
        get => _Hue;
        set
        {
            value = value > 359 ? 359 : ( value < 0 ? 0 : value );
            if ( _Hue != value )
            {
                _Hue = value;
                SyncAndNotify();
            }
        }
    }

    public int _Sat;
    public int Sat
    {
        get => _Sat;
        set
        {
            value = value > 100 ? 100 : ( value < 0 ? 0 : value );
            if ( _Sat != value )
            {
                _Sat = value;
                SyncAndNotify();
            }
        }
    }

    public int _Val;
    public int Val
    {
        get => _Val;
        set
        {
            value = value > 100 ? 100 : ( value < 0 ? 0 : value );
            if ( _Val != value )
            {
                _Val = value;
                SyncAndNotify();
            }
        }
    }

    public bool Updating { get; set; }

    // Ctor
    public ColorDataLinked()
    {
        _Hue = 0;
        _Sat = 100;
        _Val = 100;
        _Red = 255;
        _Green = 0;
        _Blue = 0;
        _PickedColor = PickedColor = Color.FromRgb( _Red, _Green, _Blue );
        _PickedColorHEX = PickedColorHEX = "#FF0000";
        Updating = false;
    }

    public void Clone( ColorDataLinked data )
    {
        Red = data.Red;
        Green = data.Green;
        Blue = data.Blue;
        Hue = data.Hue;
        Sat = data.Sat;
        Val = data.Val;
        PickedColor = data.PickedColor;
        PickedColorHEX = data.PickedColorHEX;
    }

    // Sync every value if one changes
    public void SyncAndNotify( [CallerMemberName] string? propertyName = null )
    {
        if ( Updating ) return;

        switch ( propertyName )
        {
            case "Red":
            case "Green":
            case "Blue":
                _PickedColor = Color.FromRgb( _Red, _Green, _Blue );
                _PickedColorHEX = _PickedColor.ToHex();
                UpdateHSV();
                break;
            case "Hue":
            case "Sat":
            case "Val":
                _PickedColor = Color.FromHsv( _Hue / 359F, _Sat / 100F, _Val / 100F );
                _PickedColorHEX = _PickedColor.ToHex();
                _PickedColor.ToRgb( out _Red, out _Green, out _Blue );
                break;
            case "PickedColor":
                _PickedColorHEX = _PickedColor.ToHex();
                _PickedColor.ToRgb( out _Red, out _Green, out _Blue );
                UpdateHSV();
                break;
            case "PickedColorHEX":
                UpdateByHEX();
                _PickedColor.ToRgb( out _Red, out _Green, out _Blue );
                UpdateHSV();
                break;
        }

        // Notify View to update value
        Trace.WriteLine($"{propertyName}");
        NotifyPropertyChanged();
        ParameterChanged?.Invoke( propertyName );
    }

    private void UpdateHSV()
    {
        _PickedColor.ToHsl( out float h, out float s, out float l );
        _Hue = (int)( h * 359 );
        _Sat = (int)( s * 100 );
        _Val = (int)( l * 100 );
    }

    private void UpdateByHEX()
    {
        try
        {
            _Red = byte.Parse( _PickedColorHEX[1..3], NumberStyles.HexNumber );
            _Green = byte.Parse( _PickedColorHEX[3..5], NumberStyles.HexNumber );
            _Blue = byte.Parse( _PickedColorHEX[5..7], NumberStyles.HexNumber );
        }
        catch ( ArgumentException ) // ArgumentException means not parsed correctly, return
        {
            return;
        }
        _PickedColor = Color.FromRgb( _Red, _Green, _Blue );
    }
}

public class MauiColorPicker : GraphicsView, IDrawable
{
    #region Ctor & Vars
    // Buffers for Spectum & bar
    private PictureCanvas? m_BarBuffer = null;
    private IPicture? m_BarPicture = null;
    private PictureCanvas? m_SpectumBuffer = null;
    private IPicture? m_SpectumPicture = null;

    private Point m_SpectumPickerPos = new();      // picker position
    private Point m_BarPickerPos = new();          // picker position
    private bool m_UpdateSpectum = true;                // true to create new spectum picture buffer
    private bool m_UpdateBar = true;                    // true to create new bar picture buffer

    private bool m_MouseDown = false;
    private int m_MouseDownFrom = 0;               // 0 : not down in spectum or bar rect. 1: down in spectumRect, 2: down in bar rect.

    public MauiColorPicker()
    {
        Drawable = this;
        VerticalOptions = new( LayoutAlignment.Fill, true );
        HorizontalOptions = new( LayoutAlignment.Fill, true );
        ColorData.ParameterChanged += ColorData_ParameterChanged;

        PointerGestureRecognizer pointerGestureRecognizer = new();
        pointerGestureRecognizer.PointerMoved += PointerGestureRecognizer_PointerMoved;
        pointerGestureRecognizer.PointerPressed += PointerGestureRecognizer_PointerPressed;
        pointerGestureRecognizer.PointerReleased += PointerGestureRecognizer_PointerReleased;
        GestureRecognizers.Add( pointerGestureRecognizer );
    }

    private void ColorData_ParameterChanged( string? pname )
    {
        UpdatePickerPosByColor();
        m_UpdateSpectum = true;
        m_UpdateBar = true; m_MouseDown = false;
        Invalidate();
        return;
    }
    #endregion

    #region Guestures
    private void PointerGestureRecognizer_PointerReleased( object? sender, PointerEventArgs e )
    {
        if ( m_MouseDown )
        {
            var clickPosition = e.GetPosition( this );
            if ( clickPosition == null ) return;
            CalculateTapPosition( (Point)clickPosition.Value );
        }
        m_MouseDown = false;
        m_MouseDownFrom = 0;
    }

    private void PointerGestureRecognizer_PointerPressed( object? sender, PointerEventArgs e )
    {
        if ( !m_MouseDown )
        {
            var clickPosition = e.GetPosition( this );
            if ( clickPosition == null ) return;

            if ( SpectumRect.Contains( clickPosition.Value ) )
                m_MouseDownFrom = 1;
            else if ( BarRect.Contains( clickPosition.Value ) )
                m_MouseDownFrom = 2;
            else
                m_MouseDownFrom = 0;
            CalculateTapPosition( clickPosition.Value );
        }
        m_MouseDown = true;
    }

    private void PointerGestureRecognizer_PointerMoved( object? sender, PointerEventArgs e )
    {
        if ( !m_MouseDown ) return;

        var clickPosition = e.GetPosition( this );
        if ( clickPosition == null ) return;

        CalculateTapPosition( clickPosition.Value );
    }
    #endregion

    #region Override methods for property / pointer
    // Monitor properties and ask for redraw
    protected override void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
    {
        base.OnPropertyChanged( propertyName );

        propertyName ??= string.Empty;
        switch ( propertyName )
        {
            case nameof( WidthRequest ):
            case nameof( HeightRequest ):
            case nameof( BackgroundColor ):
                break;
            case nameof( SpectumType ):
                m_UpdateSpectum = true;
                m_UpdateBar = true;
                break;
            case nameof( SpectumRadius ):
            case nameof( SpectumRect ):
                m_SpectumPickerPos = new Point( SpectumRect.X, SpectumRect.Y );
                m_UpdateSpectum = true;
                break;
            case nameof( PickerRatio ):
                break;
            case nameof( BarRect ):
            case nameof( BarVertical ):
            case nameof( BarRadius ):
                m_BarPickerPos = new Point( BarRect.X, BarRect.Y );
                m_UpdateBar = true;
                break;
            case nameof( ColorData ):
                break;
            default:
                return;
        }

        // Init redraw
        Invalidate();
    }
    #endregion

    #region Picker Pos & Value Calc
    //private Point _prev = new(0, 0);
    private void CalculateTapPosition( Point clickPosition )
    {
        if ( m_MouseDownFrom == 0 ) return;

        //var d = ( clickPosition - _prev );
        //if (Math.Abs(d.Width) + Math.Abs(d.Height) < 5 ) return;
        //_prev = clickPosition;

        // we update manually later
        ColorData.Updating = true;
        // start inside the spectum
        if ( m_MouseDownFrom == 1 )
        {
            // outside Clamp
            if ( clickPosition.X < SpectumRect.X ) clickPosition.X = SpectumRect.X;
            if ( clickPosition.Y < SpectumRect.Y ) clickPosition.Y = SpectumRect.Y;
            if ( clickPosition.X > SpectumRect.X + SpectumRect.Width ) clickPosition.X = SpectumRect.X + SpectumRect.Width;
            if ( clickPosition.Y > SpectumRect.Y + SpectumRect.Height ) clickPosition.Y = SpectumRect.Y + SpectumRect.Height;
            m_SpectumPickerPos = clickPosition;
        }
        // start inside the bar
        else if ( m_MouseDownFrom == 2 )
        {
            // outside Clamp
            if ( clickPosition.X < BarRect.X ) clickPosition.X = BarRect.X;
            if ( clickPosition.Y < BarRect.Y ) clickPosition.Y = BarRect.Y;
            if ( clickPosition.X > BarRect.X + BarRect.Width ) clickPosition.X = BarRect.X + BarRect.Width;
            if ( clickPosition.Y < BarRect.Y + BarRect.Height ) clickPosition.Y = BarRect.Y + BarRect.Height;
            m_BarPickerPos = clickPosition;

            // align picker to bar center
            if ( BarVertical ) m_BarPickerPos.X = BarRect.X + BarRect.Width / 2;
            else m_BarPickerPos.Y = BarRect.Y + BarRect.Height / 2;
        }

        // FULL_COLOR mode
        if ( SpectumType == SPECTUM_TYPE_ENUM.FULL_COLOR )
        {
            var val = BarVertical ? ( BarRect.Height - m_BarPickerPos.Y + BarRect.Y ) / BarRect.Height
                                            : ( BarRect.Width - m_BarPickerPos.X + BarRect.X ) / BarRect.Width;
            ColorData.Val = (int)( val * 100 );
            ColorData.Hue = (int)( ( m_SpectumPickerPos.X - SpectumRect.X ) * 359 / SpectumRect.Width );                            // Horizontal => Hue 0 to 359
            ColorData.Sat = (int)( ( SpectumRect.Height - ( m_SpectumPickerPos.Y - SpectumRect.Y ) ) * 100 / SpectumRect.Height );  // Vertical => Sat 100:0

            m_UpdateBar = true;
        }

        // SINGLE_COLOR mode
        else if ( SpectumType == SPECTUM_TYPE_ENUM.SINGLE_COLOR )
        {
            double hueValue = BarVertical ? ( m_BarPickerPos.Y - BarRect.Y ) / BarRect.Height
                                            : ( m_BarPickerPos.X - BarRect.X ) / BarRect.Width;
            ColorData.Hue = (int)( hueValue * 359 );
            ColorData.Sat = (int)( ( m_SpectumPickerPos.X - SpectumRect.X ) * 100 / SpectumRect.Width );                            // Horizontal => Sat 0:100
            ColorData.Val = (int)( ( SpectumRect.Height - ( m_SpectumPickerPos.Y - SpectumRect.Y ) ) * 100 / SpectumRect.Height );  // Vertical => Value 100:0

            m_UpdateSpectum = true;
        }

        // Done updating ColorData
        ColorData.Updating = false;
        ColorData.SyncAndNotify( "Hue" );
        OnPropertyChanged( nameof( ColorData ) );
        return;
    }

    private void UpdatePickerPosByColor()
    {
        if ( SpectumType == SPECTUM_TYPE_ENUM.FULL_COLOR )
        {
            m_SpectumPickerPos.X = ( ColorData.Hue / 255F * SpectumRect.Width ) + SpectumRect.X;
            m_SpectumPickerPos.Y = ( SpectumRect.Height - ColorData.Sat / 100F * SpectumRect.Height ) + SpectumRect.Y;
            if ( BarVertical )
            {
                m_BarPickerPos.X = BarRect.X + BarRect.Width / 2;
                m_BarPickerPos.Y = ColorData.Val / 100F * BarRect.Height + BarRect.Y;
            }
            else
            {
                m_BarPickerPos.X = ColorData.Val / 100F * BarRect.Width + BarRect.X;
                m_BarPickerPos.Y = BarRect.Y + BarRect.Height / 2;
            }
        }
        else if ( SpectumType == SPECTUM_TYPE_ENUM.SINGLE_COLOR )
        {
            m_SpectumPickerPos.X = ( ColorData.Sat / 100F * SpectumRect.Width ) + SpectumRect.X;
            m_SpectumPickerPos.Y = ( SpectumRect.Height - ColorData.Val / 100F * SpectumRect.Height ) + SpectumRect.Y;
            if ( BarVertical )
            {
                m_BarPickerPos.X = BarRect.X + BarRect.Width / 2;
                m_BarPickerPos.Y = ColorData.Hue / 359F * BarRect.Height + BarRect.Y;
            }
            else
            {
                m_BarPickerPos.X = ColorData.Hue / 359F * BarRect.Width + BarRect.X;
                m_BarPickerPos.Y = BarRect.Y + BarRect.Height / 2;
            }
        }
    }

    #endregion

    #region Draw
    public void Draw( ICanvas canvas, RectF dirtyRectF )    // interface
    {
        canvas.ResetState();
        DrawBackground( ref canvas, ref dirtyRectF );
        DrawSpectumRect( ref canvas, ref dirtyRectF );
        DrawBarRect( ref canvas, ref dirtyRectF );
        DrawSpectumPicker( ref canvas, ref dirtyRectF );
        DrawBarPicker( ref canvas, ref dirtyRectF );
    }

    private void DrawBackground( ref ICanvas canvas, ref RectF dirtyRect )
    {
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle( dirtyRect );
    }

    private void DrawSpectumRect( ref ICanvas canvas, ref RectF _ )
    {
        // if need to update, draw to picture & store
        if ( m_UpdateSpectum )
        {
            m_UpdateSpectum = false;

            m_SpectumBuffer?.Dispose();
            m_SpectumBuffer = new PictureCanvas( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height )
            {
                Antialias = true,
                StrokeSize = 2,
                StrokeColor = Color.FromRgb( 0, 0, 0 )
            };
            m_SpectumBuffer.DrawRoundedRectangle( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height, SpectumRadius );

            if ( SpectumType == SPECTUM_TYPE_ENUM.FULL_COLOR )
            {
                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point( 0, 0 ),
                    EndPoint = new Point( 1, 0 ),
                    GradientStops = [
                        new GradientStop {Color = Color.FromRgb(255, 0, 0), Offset=0.0000F},
                        new GradientStop {Color = Color.FromRgb(255, 255, 0), Offset=0.1666F},
                        new GradientStop {Color = Color.FromRgb(0, 255, 0), Offset=0.3333F},
                        new GradientStop {Color = Color.FromRgb(0, 255, 255), Offset=0.5000F},
                        new GradientStop {Color = Color.FromRgb(0, 0, 255), Offset=0.6666F},
                        new GradientStop {Color = Color.FromRgb(255, 0, 255), Offset=0.8333F},
                    ],
                };
                m_SpectumBuffer.SetFillPaint( gradientBrush, SpectumRect );
                m_SpectumBuffer.FillRoundedRectangle( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height, SpectumRadius );

                gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point( 0, 0 ),
                    EndPoint = new Point( 0, 1 ),   // Vertical, White Mask
                    GradientStops =
                    [
                        new GradientStop { Color = Color.FromArgb("00FFFFFF"), Offset = 0.0f }, // Start Color 0% White
                        new GradientStop { Color = Color.FromArgb("FFFFFFFF"), Offset = 1.0f }, // End Color 100% White
                    ]
                };
                m_SpectumBuffer.SetFillPaint( gradientBrush, SpectumRect );
                m_SpectumBuffer.FillRoundedRectangle( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height, SpectumRadius );

            }
            if ( SpectumType == SPECTUM_TYPE_ENUM.SINGLE_COLOR )
            {
                m_SpectumBuffer.FillColor = Color.FromHsva( (float)ColorData.Hue / 359F, 1, 1, 1 );
                m_SpectumBuffer.FillRoundedRectangle( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height, SpectumRadius );

                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point( 0, 0 ),
                    EndPoint = new Point( 1, 0 ),   // Horizontal, White Mask
                    GradientStops =
                    [
                        new GradientStop { Color = Color.FromArgb("FFFFFFFF"), Offset = 0.0f }, // Start Color 100% White
                        new GradientStop { Color = Color.FromArgb("00FFFFFF"), Offset = 1.0f }, // End Color 0% White
                    ]
                };
                m_SpectumBuffer.SetFillPaint( gradientBrush, SpectumRect );
                m_SpectumBuffer.FillRoundedRectangle( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height, SpectumRadius );

                gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point( 0, 0 ),
                    EndPoint = new Point( 0, 1 ),   // Vertical, Black Mask
                    GradientStops =
                    [
                        new GradientStop { Color = Color.FromArgb("00000000"), Offset = 0.0f }, // Start Color 0% Black
                        new GradientStop { Color = Color.FromArgb("FF000000"), Offset = 1.0f }, // End Color 100% Black
                    ],
                };
                m_SpectumBuffer.SetFillPaint( gradientBrush, SpectumRect );
                m_SpectumBuffer.FillRoundedRectangle( SpectumRect.X, SpectumRect.Y, SpectumRect.Width, SpectumRect.Height, SpectumRadius );
            }
            // Store
            m_SpectumPicture = m_SpectumBuffer.Picture;
        }

        // Use stored buffer
        m_SpectumPicture?.Draw( canvas );
    }

    private void DrawBarRect( ref ICanvas canvas, ref RectF _ )
    {
        // if need to update, draw to picture & store
        if ( m_UpdateBar )
        {
            m_UpdateBar = false;
            m_BarBuffer?.Dispose();
            m_BarBuffer = new PictureCanvas( BarRect.X, BarRect.Y, BarRect.Width, BarRect.Height )
            {
                Antialias = true,
                StrokeSize = 2,
                StrokeColor = System.Drawing.Color.Black.ToMauiColor()
            };
            m_BarBuffer.DrawRoundedRectangle( BarRect.X, BarRect.Y, BarRect.Width, BarRect.Height, BarRadius );

            if ( SpectumType == SPECTUM_TYPE_ENUM.FULL_COLOR )
            {
                canvas.FillColor = Color.FromHsv( ColorData.Hue / 359F, 1, 1 );
                m_BarBuffer.FillRoundedRectangle( BarRect.X, BarRect.Y, BarRect.Width, BarRect.Height, BarRadius );
                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point( 0, 0 ),
                    EndPoint = new Point( BarVertical ? 0 : 1, BarVertical ? 1 : 0 ),
                    GradientStops = [
                        new GradientStop { Color = Color.FromArgb("00000000"), Offset = 0.0f }, // Start Color 0% Black
                        new GradientStop { Color = Color.FromArgb("FF000000"), Offset = 1.0f }, // End Color 100% Black
                    ],
                };
                m_BarBuffer.SetFillPaint( gradientBrush, BarRect );
                m_BarBuffer.FillRoundedRectangle( BarRect.X, BarRect.Y, BarRect.Width, BarRect.Height, BarRadius );
            }
            if ( SpectumType == SPECTUM_TYPE_ENUM.SINGLE_COLOR )
            {
                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point( 0, 0 ),
                    EndPoint = new Point( BarVertical ? 0 : 1, BarVertical ? 1 : 0 ),
                    GradientStops = [
                        new GradientStop {Color = Color.FromRgb(255, 0, 0), Offset=0.0000F},
                        new GradientStop {Color = Color.FromRgb(255, 255, 0), Offset=0.1666F},
                        new GradientStop {Color = Color.FromRgb(0, 255, 0), Offset=0.3333F},
                        new GradientStop {Color = Color.FromRgb(0, 255, 255), Offset=0.5000F},
                        new GradientStop {Color = Color.FromRgb(0, 0, 255), Offset=0.6666F},
                        new GradientStop {Color = Color.FromRgb(255, 0, 255), Offset=0.8333F},
                    ],
                };
                m_BarBuffer.SetFillPaint( gradientBrush, BarRect );
                m_BarBuffer.FillRoundedRectangle( BarRect.X, BarRect.Y, BarRect.Width, BarRect.Height, BarRadius );
            }
            // store
            m_BarPicture = m_BarBuffer.Picture;
        }

        // use stored buffer
        m_BarPicture?.Draw( canvas );
    }

    private void DrawSpectumPicker( ref ICanvas canvas, ref RectF _ )
    {
        canvas.StrokeSize = 3 * PickerRatio;
        if ( ColorData.Red + ColorData.Green + ColorData.Blue < 382 ) canvas.StrokeColor = Color.FromRgb( 255, 255, 255 );
        else canvas.StrokeColor = Color.FromRgb( 0, 0, 0 );
        canvas.DrawCircle( m_SpectumPickerPos, 6D * PickerRatio );
    }

    private void DrawBarPicker( ref ICanvas canvas, ref RectF _ )
    {
        RectF indicator;
        canvas.StrokeSize = 8 * PickerRatio;
        canvas.StrokeColor = Color.FromRgb( 255, 255, 255 );
        canvas.FillColor = Color.FromRgb( 0, 0, 0 );

        if ( BarVertical )
        {
            indicator = new(
                BarRect.X,
                (float)m_BarPickerPos.Y - 5 * PickerRatio,  // make it center
                BarRect.Width,
                10 * PickerRatio
                );
        }
        else
        {
            indicator = new(
                (float)m_BarPickerPos.X - 5 * PickerRatio,    // make it center
                BarRect.Y,
                10 * PickerRatio,
                BarRect.Height
                );
        }

        canvas.DrawRoundedRectangle( indicator, 20 );
        canvas.FillRoundedRectangle( indicator, 20 );
    }
    #endregion

    #region Property: Color Data 
    public static readonly BindableProperty DataProperty =
        BindableProperty.Create( nameof( ColorData ), typeof( ColorDataLinked ), typeof( MauiColorPicker ), new ColorDataLinked(), BindingMode.OneWay );
    public ColorDataLinked ColorData
    {
        get => (ColorDataLinked)GetValue( DataProperty );
        set => SetValue( DataProperty, value );
    }
    #endregion

    #region Property: Picker
    public static readonly BindableProperty PickerPosProperty =
        BindableProperty.Create( nameof( PickerPos ), typeof( Point ), typeof( MauiColorPicker ), default, BindingMode.OneWay );
    public Point PickerPos
    {
        get => (Point)GetValue( PickerPosProperty );
        set => SetValue( PickerPosProperty, value );
    }
    #endregion

    #region Property: UI Parameters

    public static readonly BindableProperty PickerRatioProperty =
        BindableProperty.Create( nameof( PickerRatio ), typeof( float ), typeof( MauiColorPicker ), 1.0F, BindingMode.OneWay );
    public float PickerRatio
    {
        get => (float)GetValue( PickerRatioProperty );
        set => SetValue( PickerRatioProperty, value );
    }

    public enum SPECTUM_TYPE_ENUM
    {
        FULL_COLOR,
        SINGLE_COLOR
    }

    public static readonly BindableProperty SpectumTypeProperty =
        BindableProperty.Create( nameof( SpectumType ), typeof( SPECTUM_TYPE_ENUM ), typeof( MauiColorPicker ), SPECTUM_TYPE_ENUM.FULL_COLOR, BindingMode.OneTime );
    public SPECTUM_TYPE_ENUM SpectumType
    {
        get => (SPECTUM_TYPE_ENUM)GetValue( SpectumTypeProperty );
        set => SetValue( SpectumTypeProperty, value );
    }

    public static readonly BindableProperty SpectumRectProperty =
        BindableProperty.Create( nameof( SpectumRect ), typeof( RectF ), typeof( MauiColorPicker ), new RectF( 0, 0, 100, 100 ), BindingMode.OneWay );
    public RectF SpectumRect
    {
        get => (RectF)GetValue( SpectumRectProperty );
        set => SetValue( SpectumRectProperty, value );
    }

    public static readonly BindableProperty SpectumRadiusProperty =
        BindableProperty.Create( nameof( SpectumRadius ), typeof( float ), typeof( MauiColorPicker ), 0F, BindingMode.OneWay );
    public float SpectumRadius
    {
        get => (float)GetValue( SpectumRadiusProperty );
        set => SetValue( SpectumRadiusProperty, value );
    }

    public static readonly BindableProperty BarRectProperty =
        BindableProperty.Create( nameof( BarRect ), typeof( RectF ), typeof( MauiColorPicker ), new RectF( 0, 0, 100, 20 ), BindingMode.OneWay );
    public RectF BarRect
    {
        get => (RectF)GetValue( BarRectProperty );
        set => SetValue( BarRectProperty, value );
    }

    public static readonly BindableProperty BarVerticalProperty =
        BindableProperty.Create( nameof( BarVertical ), typeof( bool ), typeof( MauiColorPicker ), false, BindingMode.OneWay );
    public bool BarVertical
    {
        get => (bool)GetValue( BarVerticalProperty );
        set => SetValue( BarVerticalProperty, value );
    }

    public static readonly BindableProperty BarRadiusProperty =
        BindableProperty.Create( nameof( BarRadius ), typeof( float ), typeof( MauiColorPicker ), 0F, BindingMode.OneWay );
    public float BarRadius
    {
        get => (float)GetValue( BarRadiusProperty );
        set => SetValue( BarRadiusProperty, value );
    }

    #endregion

}