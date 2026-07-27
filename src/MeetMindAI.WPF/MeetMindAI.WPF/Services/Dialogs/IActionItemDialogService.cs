using MeetMindAI.WPF.Models.ActionItems;

namespace MeetMindAI.WPF.Services.Dialogs;

public interface IActionItemDialogService
{
    bool ShowCreate(Guid meetingId);

    bool ShowEdit(ActionItemDetails actionItem);

    bool ConfirmDelete(ActionItemDetails actionItem);
}
