using Godot;
using System;
public enum TypeUIIcon
{
	SALUD,
	ATAQUE
}

[GlobalClass]
public partial class TypeUI : Resource
{
	[Export]
	public AtlasTexture Texture;

	[Export]
	public TypeUIIcon Type;

	public TypeUI():this(null,TypeUIIcon.SALUD)
	{
	}

	public TypeUI(AtlasTexture texture, TypeUIIcon type)
	{
		Texture = texture;
		Type = type;
	}
}
