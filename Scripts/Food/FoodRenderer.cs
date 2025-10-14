using System;
using Godot;

public partial class FoodRenderer : Node2D
{
    [Export] public Vector2I GridSize = new Vector2I(740, 480);
    [Export] public Vector2 CellSize = new Vector2(4, 4);

    private Image _image;
    private ImageTexture _texture;
    private TextureRect _rect;
    private FoodGrid _foodGrid;

    public void Init(FoodGrid foodGrid)
    {
        _foodGrid = foodGrid;
    }

    private void OnFoodGridChanged()
    {
        UpdateTexture();
    }


    public override void _Ready()
    {
        _foodGrid.FoodGridChanged += OnFoodGridChanged;

        _image = Image.CreateEmpty(GridSize.X, GridSize.Y, false, Image.Format.Rgba8);
        _texture = ImageTexture.CreateFromImage(_image);

        _rect = new TextureRect
        {
            Texture = _texture,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            Size = GridSize * CellSize,
            Position = Vector2.Zero,
            Modulate = Colors.White
        };
        AddChild(_rect);
        UpdateTexture();
    }

    public void UpdateTexture()
    {
        int width = _foodGrid.Width;
        int height = _foodGrid.Height;

        byte[] pixels = new byte[width * height * 4];
        int amplification = 255;

        // Write grid data to image
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float food = Mathf.Clamp(_foodGrid.GetFood(x, y), 0f, 1f);
                int i = (y * width + x) * 4;
                pixels[i + 0] = 0;
                pixels[i + 1] = (byte)(food * amplification);
                pixels[i + 2] = 0;
                pixels[i + 3] = (byte)(food * amplification);
            }
        }

        // Update the texture
        _image.SetData(width, height, false, Image.Format.Rgba8, pixels);
        _texture.Update(_image);

        // Pass to shader material
        var material = GetNode<TextureRect>("Sprite2D").Material as ShaderMaterial;
        material.SetShaderParameter("pheromone_map", _texture);
    }
}
