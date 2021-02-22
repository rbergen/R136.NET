using R136.Entities.General;
using R136.Entities.Items;
using R136.Interfaces;
using System;

namespace R136.Core
{
	public partial class Engine : IStatusManager
	{
		public int LifePoints
			=> IsInitialized ? _player!.LifePoints : throw new InvalidOperationException(EngineNotInitialized);

		public RoomID CurrentRoom
		{
			get => IsInitialized ? _player!.CurrentRoom.ID : throw new InvalidOperationException(EngineNotInitialized);
			set
			{
				if (!IsInitialized)
					throw new InvalidOperationException(EngineNotInitialized);

				_player!.CurrentRoom = _rooms![value];
			}
		}

		public bool IsDark
		{
			get
			{
				if (!IsInitialized)
					throw new InvalidOperationException(EngineNotInitialized);

				return _player!.CurrentRoom.IsDark && (!IsInPosession(ItemID.Flashlight) || !((Flashlight)_items![ItemID.Flashlight]).IsOn);
			}
		}
		public void DecreaseHealth()
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.DecreaseHealth();
		}

		public void DecreaseHealth(HealthImpact impact)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.DecreaseHealth(impact);
		}

		public void RestoreHealth()
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.RestoreHealth();
		}

		public bool IsInPosession(ItemID item)
			=> IsInitialized ? _player!.FindInInventory(item) != null : throw new InvalidOperationException(EngineNotInitialized);

		public void RemoveFromPossession(ItemID item)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.RemoveFromInventory(item);
		}

		public void Place(ItemID item)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			if (_items![item].CurrentRoom == RoomID.None && !IsInPosession(item))
			_items![item].CurrentRoom = CurrentRoom;
		}

		public void OpenConnection(Direction direction, RoomID toRoom)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			if (!_rooms![CurrentRoom].Connections.ContainsKey(direction))
				_rooms![CurrentRoom].Connections[direction] = _rooms![toRoom];
		}

		private void TreeHasBurned()
		{
			_hasTreeBurned = true;

			if (_animates![AnimateID.GreenCrystal] is ITriggerable greenCrystal)
				greenCrystal.Trigger();

			Place(ItemID.GreenCrystal);
		}

		public void MarkAnimateTriggered()
			=> _isAnimateTriggered = true;
	}
}
