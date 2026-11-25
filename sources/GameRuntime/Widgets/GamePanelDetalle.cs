using Godot;
using System;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;

public partial class GamePanelDetalle : PanelContainer
{
	private delegate bool TryReadStatDelegate(in Entity entity, out int value);

	private readonly struct StatDescriptor
	{
		public readonly TypeUIIcon Type;
		public readonly TryReadStatDelegate TryRead;

		public StatDescriptor(TypeUIIcon type, TryReadStatDelegate tryRead)
		{
			Type = type;
			TryRead = tryRead;
		}
	}

	[Export] public int MaxNameCharacter = 23;
	[Export] public Godot.Collections.Array<TypeUI> UiStatsIcon;

	private StatDescriptor[] _descriptors = Array.Empty<StatDescriptor>();
	private AtlasTexture[] _iconLookup = Array.Empty<AtlasTexture>();
	private UiStats[] _statWidgets = Array.Empty<UiStats>();
	private int[] _lastRenderedValues = Array.Empty<int>();
	private bool[] _hasRenderedValue = Array.Empty<bool>();

	private Entity _currentEntity;
	private bool _hasEntity;

	public override void _Ready()
	{
		InitializeUI(); // Insertado por el generador de UI
		BuildStatTables();
		SetProcess(false);
	}

	private void BuildStatTables()
	{
		int count = Enum.GetValues<TypeUIIcon>().Length;

		_descriptors = new StatDescriptor[count];
		_iconLookup = new AtlasTexture[count];
		_statWidgets = new UiStats[count];
		_lastRenderedValues = new int[count];
		_hasRenderedValue = new bool[count];

		// Registro único por stat: si mañana agregas DEFENSA o VELOCIDAD, solo sumas su entrada aquí.
		RegisterDescriptor(new StatDescriptor(
			TypeUIIcon.SALUD,
			static (in Entity e, out int val) =>
			{
				if (!e.Has<HealthComponent>()) { val = 0; return false; }
				val = (int)e.Get<HealthComponent>().value;
				return true;
			}));

		RegisterDescriptor(new StatDescriptor(
			TypeUIIcon.ATAQUE_FISICO,
			static (in Entity e, out int val) =>
			{
				if (!e.Has<MeleeAttackComponent>()) { val = 0; return false; }
				val = e.Get<MeleeAttackComponent>().Damage;
				return true;
			}));

		RegisterDescriptor(new StatDescriptor(
			TypeUIIcon.CANTIDAD_RECURSOS,
			static (in Entity e, out int val) =>
			{
				if (!e.Has<AmountComponent>()) { val = 0; return false; }
				val = e.Get<AmountComponent>().value;
				return true;
			}));

		if (UiStatsIcon == null) return;

		for (int i = 0; i < UiStatsIcon.Count; i++)
		{
			TypeUI entry = UiStatsIcon[i];
			if (entry == null) continue;

			int index = (int)entry.Type;
			if ((uint)index < (uint)count)
			{
				_iconLookup[index] = entry.Texture;
			}
		}
	}

	private void RegisterDescriptor(in StatDescriptor descriptor)
	{
		int index = (int)descriptor.Type;
		if ((uint)index < (uint)_descriptors.Length)
		{
			_descriptors[index] = descriptor;
		}
	}

	public void SetEntity(Entity entity)
	{
		_currentEntity = entity;

		for (int i = 0; i < _statWidgets.Length; i++)
		{
			_hasRenderedValue[i] = false;
			if (_statWidgets[i] != null)
			{
				_statWidgets[i].Visible = false;
			}
		}

		if (!_currentEntity.IsAlive())
		{
			_hasEntity = false;
			SetProcess(false);
			return;
		}

		_hasEntity = true;
		SyncAllStats(forceUpdate: true);
		SetProcess(Visible);
	}

	private void SyncAllStats(bool forceUpdate)
	{
		for (int i = 0; i < _descriptors.Length; i++)
		{
			TryReadStatDelegate tryRead = _descriptors[i].TryRead;
			if (tryRead == null) continue;

			if (!tryRead(in _currentEntity, out int currentValue))
			{
				if (_statWidgets[i] != null)
				{
					_statWidgets[i].Visible = false;
				}
				_hasRenderedValue[i] = false;
				continue;
			}

			UiStats widget = EnsureStatWidget(i);
			widget.Visible = true;

			if (forceUpdate || !_hasRenderedValue[i] || _lastRenderedValues[i] != currentValue)
			{
				_lastRenderedValues[i] = currentValue;
				_hasRenderedValue[i] = true;
				widget.SetValue(currentValue);
			}
		}
	}

	private UiStats EnsureStatWidget(int index)
	{
		UiStats widget = _statWidgets[index];
		if (widget == null)
		{
			widget = RuntimeServices.NodeRegistry.Create<UiStats>();
			Contenedor.AddChild(widget);

			AtlasTexture icon = _iconLookup[index];
			if (icon != null)
			{
				widget.SetIcon(icon);
			}

			_statWidgets[index] = widget;
		}

		return widget;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationVisibilityChanged)
		{
			SetProcess(Visible && _hasEntity);
		}
	}

	public override void _Process(double delta)
	{
		if (!_hasEntity || !Visible) return;

		if (!_currentEntity.IsAlive())
		{
			_hasEntity = false;
			SetProcess(false);
			return;
		}

		SyncAllStats(forceUpdate: false);
	}
}