﻿using System;
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ObservableItemsSource : IItemsViewSource
	{
		readonly UICollectionView _collectionView;
		readonly IList _itemsSource;

		public ObservableItemsSource(IEnumerable itemSource, UICollectionView collectionView)
		{
			_collectionView = collectionView;
			_itemsSource = (IList)itemSource;

			((INotifyCollectionChanged)itemSource).CollectionChanged += CollectionChanged;
		}

		public int Count => _itemsSource.Count;

		public object this[int index] => _itemsSource[index];

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (count == 1)
			{
				// For a single item, we can use MoveItem and get the animation
				var oldPath = NSIndexPath.Create(0, args.OldStartingIndex);
				var newPath = NSIndexPath.Create(0, args.NewStartingIndex);

				_collectionView.MoveItem(oldPath, newPath);
				return;
			}

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;
			_collectionView.ReloadItems(CreateIndexesFrom(start, end));
		}
		
		private void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);

				// We are replacing one set of items with a set of equal size; we can do a simple item range update
				_collectionView.ReloadItems(CreateIndexesFrom(startIndex, newCount));
				return;
			}
			
			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use ReloadData and let the UICollectionView update everything
			_collectionView.ReloadData();
		}

		static NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			var result = new NSIndexPath[count];

			for (int n = 0; n < count; n++)
			{
				result[n] = NSIndexPath.Create(0, startIndex + n);
			}

			return result;
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);
			var count = args.NewItems.Count;

			_collectionView.InsertItems(CreateIndexesFrom(startIndex, count));
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a ReloadData()
				_collectionView.ReloadData();
				return;
			}

			// If we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count;
			_collectionView.DeleteItems(CreateIndexesFrom(startIndex, count));
		}
	}

	internal class BasicGroupedSource : IGroupedItemsViewSource
	{
		readonly UICollectionView _collectionView;
		readonly IList _groupSource;

		public BasicGroupedSource(IList groupSource, UICollectionView collectionView)
		{
			_collectionView = collectionView;
			_groupSource = groupSource;
		}

		public object this[int itemIndex]
		{
			get
			{
				var current = itemIndex;

				for (int group = 0; group < _groupSource.Count; group++)
				{
					var currentGroup = (IList)_groupSource[group];
					if (current < currentGroup.Count)
					{
						return currentGroup[current];
					}

					current -= currentGroup.Count;
				}

				throw new ArgumentOutOfRangeException(nameof(itemIndex));
			}
		}

		public object this[NSIndexPath indexPath] => ((IList)_groupSource[indexPath.Section])[indexPath.Row];

		public int Count
		{
			get
			{
				var count = 0;

				for (int group = 0; group < _groupSource.Count; group++)
				{
					if (_groupSource[group] is IList groupItems)
					{
						count += groupItems.Count;
					}
				}

				return count;
			}
		}

		public int GroupCount => _groupSource.Count;

		public int CountInGroup(int group)
		{
			return ((IList)_groupSource[group]).Count;
		}
	}
}