using Godot;
using System;
using System.Collections.Generic;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;

public partial class GamePanelDetalle : PanelContainer
{
	[Export] public int MaxNameCharacter = 23;
	[Export] public Godot.Collections.Array<TypeUI> UiStatsIcon;

	private AtlasTexture[] _iconLookup = Array.Empty<AtlasTexture>();
	private readonly Dictionary<TypeUIIcon, UiStats> _activeStatWidgets = new();
	private readonly Dictionary<TypeUIIcon, int> _lastRenderedValues = new();

	private Entity _currentEntity;
	private bool _hasEntity;

	public override void _Ready()
	{
		InitializeUI(); // Insertado por el generador de UI
		BuildIconLookup();
		SetProcess(false);
	}

	private void BuildIconLookup()
	{
		int count = Enum.GetValues<TypeUIIcon>().Length;
		_iconLookup = new AtlasTexture[count];

		if (UiStatsIcon == null) return;

		for (int i = 0; i < UiStatsIcon.Count; i++)
		{
			TypeUI entry = UiStatsIcon[i];
			if (entry == null) continue;

			int index = (int)entry.Type;
			if ((uint)index < (uint)_iconLookup.Length)
			{
				_iconLookup[index] = entry.Texture;
			}
		}
	}

	public void SetEntity(Entity entity)
	{
		_currentEntity = entity;
		_lastRenderedValues.Clear();

		foreach (var kvp in _activeStatWidgets)
		{
			kvp.Value.Visible = false;
		}

		if (!_currentEntity.IsAlive())
		{
			_hasEntity = false;
			SetProcess(false);
			return;
		}

		_hasEntity = true;

		if (_currentEntity.Has<HealthComponent>())
		{
			ref readonly var health = ref _currentEntity.Get<HealthComponent>();
			int val = (int)health.value; // Ajusta al nombre real de tu campo (Current, Value, etc.)
			EnsureStatWidget(TypeUIIcon.SALUD, val);
			_lastRenderedValues[TypeUIIcon.SALUD] = val;
		}

		if (_currentEntity.Has<MeleeAttackComponent>())
		{
			ref readonly var attack = ref _currentEntity.Get<MeleeAttackComponent>();
			int val = attack.Damage; // Ajusta al nombre real de tu campo
			EnsureStatWidget(TypeUIIcon.ATAQUE_FISICO, val);
			_lastRenderedValues[TypeUIIcon.ATAQUE_FISICO] = val;
		}

		SetProcess(Visible);
	}

	private void EnsureStatWidget(TypeUIIcon statType, int initialValue)
	{
		if (!_activeStatWidgets.TryGetValue(statType, out UiStats widget))
		{
			
			widget = RuntimeServices.NodeRegistry.Create<UiStats>();
			Contenedor.AddChild(widget);

			AtlasTexture icon = _iconLookup[(int)statType];
			if (icon != null)
			{
				widget.SetIcon(icon);
			}

			_activeStatWidgets[statType] = widget;
		}

		widget.Visible = true;
		widget.SetValue(initialValue);
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

		if (_activeStatWidgets.TryGetValue(TypeUIIcon.SALUD, out UiStats hpWidget) &&
			hpWidget.Visible &&
			_currentEntity.Has<HealthComponent>())
		{
			ref readonly var health = ref _currentEntity.Get<HealthComponent>();
			int currentHp = (int)health.value;

			if (!_lastRenderedValues.TryGetValue(TypeUIIcon.SALUD, out int lastHp) || lastHp != currentHp)
			{
				_lastRenderedValues[TypeUIIcon.SALUD] = currentHp;
				hpWidget.SetValue(currentHp);
			}
		}

		if (_activeStatWidgets.TryGetValue(TypeUIIcon.ATAQUE_FISICO, out UiStats atkWidget) &&
			atkWidget.Visible &&
			_currentEntity.Has<MeleeAttackComponent>())
		{
			ref readonly var attack = ref _currentEntity.Get<MeleeAttackComponent>();
			int currentAtk = attack.Damage;

			if (!_lastRenderedValues.TryGetValue(TypeUIIcon.ATAQUE_FISICO, out int lastAtk) || lastAtk != currentAtk)
			{
				_lastRenderedValues[TypeUIIcon.ATAQUE_FISICO] = currentAtk;
				atkWidget.SetValue(currentAtk);
			}
		}
	}
}