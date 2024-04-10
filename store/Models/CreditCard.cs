using System;
namespace store.Models
{
	public class CreditCard
	{
        public string CardNumber { get; set; }
        public string CardDate { get; set; }
        public string CardCvv { get; set; }
        public string UserEmail { get; set; }

        public CreditCard() { }

        public CreditCard(string useremail, string cardnumber,string carddate,string cvv)
        {
            UserEmail = useremail;
            CardNumber = cardnumber;
            CardDate = carddate;
            CardCvv = cvv;
        }
    }
}

