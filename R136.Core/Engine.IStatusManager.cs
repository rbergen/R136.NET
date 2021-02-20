using R136.Entities.General;
using R136.Entities.Items;
using R136.Interfaces;
using System;

namespace R136.Core
{
	public partial class Engine : IStatusManager
	{
		public int LifePoints
			=> Initialized ? _player!.LifePoints : throw new InvalidOperationException(EngineNotInitialized);

		public RoomID CurrentRoom
		{
			get => Initialized ? _player!.CurrentRoom.ID : throw new InvalidOperationException(EngineNotInitialized);
			set
			{
				if (!Initialized)
					throw new InvalidOperationException(EngineNotInitialized);

				_player!.CurrentRoom = _rooms![value];
			}
		}

		public bool IsDark
		{
			get
			{
				if (!Initialized)
					throw new InvalidOperationException(EngineNotInitialized);

				return _player!.CurrentRoom.IsDark && (!IsInPosession(ItemID.Flashlight) || !((Flashlight)_items![ItemID.Flashlight]).IsOn);
			}
		}
		public void DecreaseHealth()
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.DecreaseHealth();
		}

		public void DecreaseHealth(HealthImpact impact)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.DecreaseHealth(impact);
		}

		public void RestoreHealth()
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.RestoreHealth();
		}

		public bool IsInPosession(ItemID item)
			=> Initialized ? _player!.FindInInventory(item) != null : throw new InvalidOperationException(EngineNotInitialized);

		public void RemoveFromPossession(ItemID item)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.RemoveFromInventory(item);
		}

		public void PutDown(ItemID item)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_items![item].CurrentRoom = CurrentRoom;
		}

		public void OpenConnection(Direction direction, RoomID toRoom)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_rooms![CurrentRoom].Connections[direction] = _rooms![toRoom];
		}

		private void TreeHasBurned()
		{
			_treeHasBurned = true;

			if (_animates![AnimateID.GreenCrystal] is ITriggerable greenCrystal)
				greenCrystal.Trigger();

			PutDown(ItemID.GreenCrystal);
		}

		public void MarkAnimateTriggered()
			=> _isAnimateTriggered = true;
	}
}
