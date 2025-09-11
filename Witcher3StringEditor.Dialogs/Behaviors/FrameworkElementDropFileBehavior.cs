using System.Windows;
using Microsoft.Xaml.Behaviors;
using WinRT;

namespace Witcher3StringEditor.Dialogs.Behaviors;

public class FrameworkElementDropFileBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data),
            typeof(string[]),
            typeof(FrameworkElementDropFileBehavior),
            new PropertyMetadata(null));

    public string[] Data
    {
        get => (string[])GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.DragEnter += AssociatedObject_DragEnterOrDragLeave;
        AssociatedObject.DragLeave += AssociatedObject_DragEnterOrDragLeave;
        AssociatedObject.Drop += AssociatedObject_Drop;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DragEnter -= AssociatedObject_DragEnterOrDragLeave;
        AssociatedObject.DragLeave -= AssociatedObject_DragEnterOrDragLeave;
        AssociatedObject.Drop -= AssociatedObject_Drop;
    }

    private static void AssociatedObject_DragEnterOrDragLeave(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void AssociatedObject_Drop(object sender, DragEventArgs e)
    {
        Data = e.Data.GetData(DataFormats.FileDrop).As<string[]>();
    }
}