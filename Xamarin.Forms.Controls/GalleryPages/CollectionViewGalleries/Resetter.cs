namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class Resetter<T> : MultiTestObservableCollectionModifier<T>
	{
		public Resetter(CollectionView cv) : base(cv, "Reset")
		{
		}

		protected override void ModifyObservableCollection(MultiTestObservableCollection<T> observableCollection, params int[] indexes)
		{
			observableCollection.TestReset();
		}
	}
}