using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;

public partial class ControlSpriteEdit : MarginContainer
{


    public delegate void RequestNotifyPreviewShape(List<Vector2> PointsPoligon);
    public event RequestNotifyPreviewShape OnNotifyPreviewShape;

    public delegate void RequestNotifyMovePosition(Vector2 PositionShape, Vector2 size);
    public event RequestNotifyMovePosition OnNotifyPositionShape;

    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		DrawSquare(new Vector2(0,0), new Vector2(100,100));
        CheckButtonModeGrid.Pressed += CheckButtonModeGrid_Pressed;
        ShapesForm.OnNotifyPreviewShape += ShapesForm_OnNotifyPreviewShape;
        ShapesForm.OnNotifyPositionShape += ShapesForm_OnNotifyPositionShape;
    }

    private void ShapesForm_OnNotifyPositionShape(Vector2 PositionShape, Vector2 size)
    {
        this.OnNotifyPositionShape?.Invoke(PositionShape,size);
    }

    private void ShapesForm_OnNotifyPreviewShape(List<Vector2> PointsPoligon)
    {
        this.OnNotifyPreviewShape?.Invoke(PointsPoligon);
    }

    Vector2I lastSize = Vector2I.Zero;
    private void CheckButtonModeGrid_Pressed()
    {
        lastSize = new Vector2I((int)SpinBoxX.Value, (int)SpinBoxY.Value);
        if (CheckButtonModeGrid.ButtonPressed)
		{
            SetModeGrid(GridDrawMode.CenterTile);
			SetCellSize(16, 16);
            CheckButtonModeGrid.Text = "Grid Centrado";

        }
		else
		{
			SetCellSize(lastSize.X, lastSize.Y);
            SetModeGrid(GridDrawMode.CenterLines);
			CheckButtonModeGrid.Text = "Grid Líneas";
        }
    }

    
    Vector2 positionOffset = Vector2.Zero;

	float centerOffsetY = 0f;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
	int indexFrame = 0;
	float currentfps = 0f;
    float frameDuration = 0.1f;

	bool isloopAnimation = true;
    bool isPlayingAnimation = true;
    public override void _Process(double delta)
	{
		if (!isPlayingAnimation)
		{
			return;
		}
        currentfps += (float)delta;        
        // Actualizar animación si existe
        if (animationData != null && animationData.Count > 0)
		{			
            if (currentfps >= frameDuration)
            {
                currentfps = 0;
                var currentTileInfo = animationData[indexFrame];
                TextureImage.Texture = currentTileInfo.texture;
                indexFrame++;
            }
            if (indexFrame >= animationData.Count && isloopAnimation)
            {
                indexFrame = 0;
            }            		
        }
    }
    public void SetBlockedTile(int x, int y)
    {
        TileOcupancy.BlockTile(x,y);
    }
    public void UnsetBlockedTile(int x, int y)
    {
        TileOcupancy.UnblockTile(x,y);
    }
    public void SetSelectedTile(int x, int y)
    {
        TileOcupancy.SelectTile(x,y);
    }
    public void UnsetSelectedTile(int x, int y)
    {
        TileOcupancy.UnselectTile(x,y);
    }
    public List<KuroTile> GetSelectedTiles()
    {
        return TileOcupancy.GetSelectedTiles();
    }
    public void SetSelectedTiles(List<KuroTile> tiles)
    {
        TileOcupancy.SetSelectedTiles(tiles);
    }

    public void SetModeGrid(GridDrawMode mode)
	{
		GridGeneric.SetDrawMode(mode);
    }

    public void SetLoopAnimation(bool loop)
	{
		isloopAnimation = loop;
    }
    public void SetFpsAnimation(float fps)
	{
		frameDuration = fps;
    }
    public void SetCellSize(int width, int height)
	{		
        GridGeneric.SetCellSize(width, height);
    }
	public void SetTexture(Texture2D texture2D)
	{
		TextureImage.Texture = texture2D;
		TextureImage.Size = texture2D.GetSize();
		TextureImage.Position = -TextureImage.GetSize() / 2f;
		isPlayingAnimation = false;
    }
	List<TileInfoKuro> animationData = null;

    public void Clear()
    {
        TextureImage.Texture = null;
        if (animationData!=null)
        {
            animationData.Clear();
            indexFrame = 0;
            currentfps = 0;
            isPlayingAnimation = false;
        }
        
    }
    public void SetTextureAnimation(List<TileInfoKuro> tileInfoKuros)
	{
		if (tileInfoKuros == null || tileInfoKuros.Count == 0)
			return;
        indexFrame = 0;
        currentfps = 0;
		isPlayingAnimation = true;
        var texture2D = tileInfoKuros[0].texture;
        TextureImage.Texture = texture2D;
        TextureImage.Size = texture2D.GetSize();
        TextureImage.Position = -TextureImage.GetSize() / 2f;
        animationData = tileInfoKuros;
    }

	public void SetScaleTexture(float scale)
	{
		TextureImage.Scale = new Vector2(scale, scale);
        Vector2 center = -(TextureImage.GetSize() * TextureImage.Scale) / 2f;
        TextureImage.Position = center + positionOffset;
    }	
    public void SetOffsetPositionTexture(Vector2 Offset)
	{
		positionOffset = Offset;
        Vector2 center = -(TextureImage.GetSize()* TextureImage.Scale) / 2f;
        TextureImage.Position = center + positionOffset;
    }
	public void ClearDraw()
	{
		ShapesForm.shapeType = ShapesForm.ShapeType.Ninguno;
    }

	public void SetPositionShape(Vector2 position)
	{
		ShapesForm.positionShape = position;
    }
    public void DrawCircle(Vector2 center, float radius)
	{
		ShapesForm.shapeType = ShapesForm.ShapeType.Circulo;
		ShapesForm.positionShape = center;
		ShapesForm.radiusShape = radius;
    }

	public void DrawSquare(Vector2 center, Vector2 size)
	{
		ShapesForm.shapeType = ShapesForm.ShapeType.Cuadrado;
		ShapesForm.positionShape = center;
		ShapesForm.sizeShape = size;
    }

    internal void PaintDrawPolygon(List<Vector2> points, bool invertSign=false)
    {
        ShapesForm.shapeType = ShapesForm.ShapeType.Poligono;
        List<Vector2> pointsNormalized = new List<Vector2>();
        if (invertSign)
        {
            foreach (var item in points)
            {
                Vector2 nv = new Vector2(item.X, item.Y * (-1));
                pointsNormalized.Add(nv);
            }
        }
        ShapesForm.SetPoligonPoints(pointsNormalized);
    }
    internal void EnablePaintDrawPolygon()
    {
        ShapesForm.shapeType = ShapesForm.ShapeType.Poligono;
    }
    public void SetOffsetCenterY(float y)
	{ 
		centerOffsetY = y;
		Vector2 cent = -(center.GetSize()) / 2f;
        center.Position = cent+ new Vector2(0, y);
    }
    public void SetFlipY(bool v)
    {
        TextureImage.FlipV = v;
    }
	public void SetFlipX(bool v)
	{
		TextureImage.FlipH = v;
    }


}
