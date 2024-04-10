using System;
namespace store.Models

{
    public class Notification
    {
        public List<Notify> List_Notification { get; set; }

        public Notification()
        {
            this.List_Notification = new List<Notify>();
        }

        public Notification(List<Notify> list)
        {
            this.List_Notification = list;
        }
    }
}
