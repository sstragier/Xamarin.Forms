using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Xamarin.Forms.Internals;
using Path = System.IO.Path;
using AndroidAppCompat = Android.Support.V7.Content.Res.AppCompatResources;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.Android
{
	public static class ResourceManager
	{
		public static Type DrawableClass { get; set; }

		public static Type ResourceClass { get; set; }

		public static Type StyleClass { get; set; }

		public static Type LayoutClass { get; set; }

		internal static async Task<Drawable> GetFormsDrawableAsync(this Context context, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (imageSource == null || imageSource.IsEmpty)
				return null;

			// try take a shortcut for files
			if (imageSource is FileImageSource fileImageSource)
			{
				var file = fileImageSource.File;
				var id = IdFromTitle(file, DrawableClass);

				// try the drawables via id
				if (id != 0)
				{
					var drawable = AndroidAppCompat.GetDrawable(context, id);
					if (drawable != null)
						return drawable;
				}

				// try a direct file on the file system
				if (File.Exists(file))
				{
					using (var bitmap = await BitmapFactory.DecodeFileAsync(file).ConfigureAwait(false))
					{
						if (bitmap != null)
							return new BitmapDrawable(context.Resources, bitmap);
					}
				}

				// try the bitmap resources via id
				if (id != 0)
				{
					using (var bitmap = await BitmapFactory.DecodeResourceAsync(context.Resources, id).ConfigureAwait(false))
					{
						if (bitmap != null)
							return new BitmapDrawable(context.Resources, bitmap);
					}
				}
			}

			// fall back to the handler
			using (var bitmap = await context.GetFormsBitmapAsync(imageSource, cancellationToken).ConfigureAwait(false))
			{
				if (bitmap != null)
					return new BitmapDrawable(context.Resources, bitmap);
			}

			return null;
		}

		internal static async Task<Bitmap> GetFormsBitmapAsync(this Context context, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (imageSource == null || imageSource.IsEmpty)
				return null;

			var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadImageAsync(imageSource, context, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// no-op
			}

			return null;
		}

		internal static Task ApplyDrawableAsync(this IShellContext shellContext, BindableObject bindable, BindableProperty imageSourceProperty, Action<Drawable> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			var renderer = shellContext as IVisualElementRenderer ?? throw new InvalidOperationException($"The shell context {shellContext.GetType()} must be a {typeof(IVisualElementRenderer)}.");

			return renderer.ApplyDrawableAsync(bindable, imageSourceProperty, shellContext.AndroidContext, onSet, onLoading, cancellationToken);
		}

		internal static Task ApplyDrawableAsync(this IVisualElementRenderer renderer, BindableProperty imageSourceProperty, Context context, Action<Drawable> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return renderer.ApplyDrawableAsync(null, imageSourceProperty, context, onSet, onLoading, cancellationToken);
		}

		internal static async Task ApplyDrawableAsync(this IVisualElementRenderer renderer, BindableObject bindable, BindableProperty imageSourceProperty, Context context, Action<Drawable> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = renderer ?? throw new ArgumentNullException(nameof(renderer));
			_ = context ?? throw new ArgumentNullException(nameof(context));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			// TODO: it might be good to make sure the renderer has not been disposed

			// makse sure things are good before we start
			var element = bindable ?? renderer.Element;

			if (element == null || renderer.View == null)
				return;

			onLoading?.Invoke(true);
			if (element.GetValue(imageSourceProperty) is ImageSource initialSource)
			{
				try
				{
					using (var drawable = await context.GetFormsDrawableAsync(initialSource))
					{
						// TODO: it might be good to make sure the renderer has not been disposed

						// we are back, so update the working element
						element = bindable ?? renderer.Element;

						// makse sure things are good now that we are back
						if (element == null || renderer.View == null)
							return;

						// only set if we are still on the same image
						if (element.GetValue(imageSourceProperty) == initialSource)
							onSet(drawable);
					}
				}
				finally
				{
					if (element != null && onLoading != null)
					{
						// only mark as finished if we are still on the same image
						if (element.GetValue(imageSourceProperty) == initialSource)
							onLoading.Invoke(false);
					}
				}
			}
			else
			{
				onSet(null);
				onLoading?.Invoke(false);
			}
		}

		internal static async Task ApplyDrawableAsync(this Context context, BindableObject bindable, BindableProperty imageSourceProperty, Action<Drawable> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
			_ = bindable ?? throw new ArgumentNullException(nameof(bindable));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			onLoading?.Invoke(true);
			if (bindable.GetValue(imageSourceProperty) is ImageSource initialSource)
			{
				try
				{
					using (var drawable = await context.GetFormsDrawableAsync(initialSource))
					{
						// only set if we are still on the same image
						if (bindable.GetValue(imageSourceProperty) == initialSource)
							onSet(drawable);
					}
				}
				finally
				{
					if (onLoading != null)
					{
						// only mark as finished if we are still on the same image
						if (bindable.GetValue(imageSourceProperty) == initialSource)
							onLoading.Invoke(false);
					}
				}
			}
			else
			{
				onSet(null);
				onLoading?.Invoke(false);
			}
		}

		public static Bitmap GetBitmap(this Resources resource, FileImageSource fileImageSource)
		{
			var file = fileImageSource.File;

			var bitmap = GetBitmap(resource, file);
			if (bitmap != null)
				return bitmap;

			return BitmapFactory.DecodeFile(file);
		}

		public static Bitmap GetBitmap(this Resources resource, string name)
		{
			return BitmapFactory.DecodeResource(resource, IdFromTitle(name, DrawableClass));
		}

		public static Task<Bitmap> GetBitmapAsync(this Resources resource, string name)
		{
			return BitmapFactory.DecodeResourceAsync(resource, IdFromTitle(name, DrawableClass));
		}

		[Obsolete("GetDrawable(this Resources, string) is obsolete as of version 2.5. "
			+ "Please use GetDrawable(this Context, string) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Drawable GetDrawable(this Resources resource, string name)
		{
			int id = IdFromTitle(name, DrawableClass);
			if (id == 0)
			{
				Log.Warning("Could not load image named: {0}", name);
				return null;
			}

			return AndroidAppCompat.GetDrawable(Forms.Context, id);
		}

		public static Drawable GetDrawable(this Context context, string name)
		{
			int id = IdFromTitle(name, DrawableClass);
			if (id == 0)
			{
				Log.Warning("Could not load image named: {0}", name);
				return null;
			}

			return AndroidAppCompat.GetDrawable(context, id);
		}

		public static int GetDrawableByName(string name)
		{
			return IdFromTitle(name, DrawableClass);
		}

		public static int GetResourceByName(string name)
		{
			return IdFromTitle(name, ResourceClass);
		}

		public static int GetLayoutByName(string name)
		{
			return IdFromTitle(name, LayoutClass);
		}

		public static int GetStyleByName(string name)
		{
			return IdFromTitle(name, StyleClass);
		}

		public static void Init(Assembly masterAssembly)
		{
			DrawableClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Drawable" || x.Name == "Resource_Drawable");
			ResourceClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Id" || x.Name == "Resource_Id");
			StyleClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Style" || x.Name == "Resource_Style");
			LayoutClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Layout" || x.Name == "Resource_Layout");
		}

		internal static int IdFromTitle(string title, Type type)
		{
			string name = Path.GetFileNameWithoutExtension(title);
			int id = GetId(type, name);
			return id;
		}

		static int GetId(Type type, string memberName)
		{
			object value = null;
			var fields = type.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				if (field.Name == memberName)
				{
					value = field.GetValue(type);
					break;
				}
			}

			if (value == null)
			{
				var properties = type.GetProperties();
				for (int i = 0; i < properties.Length; i++)
				{
					var prop = properties[i];
					if (prop.Name == memberName)
					{
						value = prop.GetValue(type);
						break;
					}
				}
			}

			if (value is int result)
				return result;
			return 0;
		}
	}
}
