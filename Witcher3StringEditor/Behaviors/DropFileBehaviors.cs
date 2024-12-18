using CommandLine;
using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace Witcher3StringEditor.Behaviors
{
    internal class DropFileBehaviors : Behavior<FrameworkElement>
    {
        public string[] Data
        {
            get => (string[])GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(string[]), typeof(DropFileBehaviors), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
            AssociatedObject.Drop -= AssociatedObject_Drop;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            Data = e.Data.GetData(DataFormats.FileDrop).Cast<string[]>();
        }
    }
}