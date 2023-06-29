using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

public static class DragDropHelper
{
    public static void RegisterDragDrop(this UIView view, Func<string, Task>? content)
    {
        var dropInteraction = new UIDropInteraction(new DropInteractionDelegate()
        {
            Content = content
        });
        view.AddInteraction(dropInteraction);
    }

    public static void UnRegisterDragDrop(this UIView view)
    {
        var dropInteractions = view.Interactions.OfType<UIDropInteraction>();
        foreach (var interaction in dropInteractions)
        {
            view.RemoveInteraction(interaction);
        }
    }
}

class DropInteractionDelegate : UIDropInteractionDelegate
{
    public Func<string, Task>? Content { get; init; }

    public override UIDropProposal SessionDidUpdate(UIDropInteraction interaction, IUIDropSession session)
    {
        return new UIDropProposal(UIDropOperation.Copy);
    }

    public override void PerformDrop(UIDropInteraction interaction, IUIDropSession session)
    {
        foreach (var item in session.Items)
        {
            item.ItemProvider.LoadItem(UniformTypeIdentifiers.UTTypes.Json.Identifier, null, async (data, error) =>
            {
                if (data is NSUrl nsData && !string.IsNullOrEmpty(nsData.Path))
                {
                    if (Content is not null)
                    {
                        //var bytes = await File.ReadAllBytesAsync(nsData.Path);
                        await Content.Invoke(nsData.Path);
                    }
                }
            });
        }
    }
}

