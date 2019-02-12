namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal abstract class MultiTestObservableCollectionModifier<T> : CollectionModifier
	{
		protected MultiTestObservableCollectionModifier(CollectionView cv, string buttonText) : base(cv, buttonText)
		{
		}

		protected override void OnButtonClicked()
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}

			if (_cv.ItemsSource is MultiTestObservableCollection<T> observableCollection)
			{
				ModifyObservableCollection(observableCollection, indexes);
			}
		}

		protected abstract void ModifyObservableCollection(MultiTestObservableCollection<T> observableCollection, params int[] indexes);
	}
}