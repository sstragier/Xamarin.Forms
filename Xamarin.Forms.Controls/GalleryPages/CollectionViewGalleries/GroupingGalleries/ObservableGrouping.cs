using System;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.GroupingGalleries
{
	internal class ObservableGrouping : ContentPage
	{
		public ObservableGrouping()
		{

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemsLayout = new ListItemsLayout(ItemsLayoutOrientation.Vertical) as IItemsLayout;

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = ItemTemplate(),
				GroupFooterTemplate = GroupFooterTemplate(),
				GroupHeaderTemplate = GroupHeaderTemplate(),
				IsGroupingEnabled = true
			};

			collectionView.ItemsSource = new ObservableSuperTeams();

			var remover = new MultiItemRemover<CollectionViewGalleryTestItem>(collectionView, true);

			var adder = new MultiItemAdder<CollectionViewGalleryTestItem>(collectionView, 
				(n) => new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n), $"Added", "coffee.png", n), true);
			var replacer = new MultiItemReplacer<CollectionViewGalleryTestItem>(collectionView,
				(n) => new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n), $"Added", "coffee.png", n));
			var mover = new MultiItemMover<CollectionViewGalleryTestItem>(collectionView);

			layout.Children.Add(remover);
			Grid.SetRow(remover, 0);

			layout.Children.Add(adder);
			Grid.SetRow(adder, 1);

			layout.Children.Add(replacer);
			Grid.SetRow(replacer, 2);

			layout.Children.Add(mover);
			Grid.SetRow(mover, 3);

			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 4);

			Content = layout;
		}

		DataTemplate ItemTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout();
				var label = new Label()
				{
					Margin = new Thickness(5, 0, 0, 0),

				};

				label.SetBinding(Label.TextProperty, new Binding("Name"));
				layout.Children.Add(label);

				return layout;
			});
		}

		DataTemplate GroupHeaderTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout();
				var label = new Label()
				{
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Color.LightGreen

				};

				label.SetBinding(Label.TextProperty, new Binding("Name"));
				layout.Children.Add(label);

				return layout;
			});
		}

		DataTemplate GroupFooterTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout();
				var label = new Label()
				{
					Margin = new Thickness(0, 0, 0, 15),
					BackgroundColor = Color.LightBlue

				};

				label.SetBinding(Label.TextProperty, new Binding("Name", stringFormat: "Total members: {0:D}"));
				layout.Children.Add(label);

				return layout;
			});
		}
	}
}
