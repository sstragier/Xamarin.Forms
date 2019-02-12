using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class MultiItemAdder<T> : MultiTestObservableCollectionModifier<T>
	{
		private readonly Func<int, T> _itemCreator;
		readonly bool _withIndex;

		public MultiItemAdder (CollectionView cv, Func<int, T> itemCreator, bool withIndex = false) : base(cv, "Add 4 Items")
		{
			_itemCreator = itemCreator;
			_withIndex = withIndex;
		}

		protected override void ModifyObservableCollection(MultiTestObservableCollection<T> observableCollection, params int[] indexes)
		{
			var index1 = indexes[0];

			if (index1 > -1 && index1 < observableCollection.Count)
			{
				var newItems = new List<T>();

				for (int n = 0; n < 4; n++)
				{
					newItems.Add(_itemCreator(n));
				}
				
				if (_withIndex)
				{
					observableCollection.TestAddWithListAndIndex(newItems, index1);
				}
				else
				{
					observableCollection.TestAddWithList(newItems, index1);
				}
			}
		}
	}
}