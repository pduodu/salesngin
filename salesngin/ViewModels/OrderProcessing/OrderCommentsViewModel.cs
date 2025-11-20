namespace salesngin.ViewModels.OrderProcessing;

public class OrderCommentsViewModel : BaseViewModel
{
    public ApplicationUser LoggedInUser { get; set; }
    public List<OrderComment> OrderComments { get; set; }

}
