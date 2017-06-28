using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.InAppBilling;
using System.Collections.Generic;

namespace IAP.Android
{
    [Activity(Label = "Demo In-App Purchases", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        IList<Product> _products;
        private InAppBillingServiceConnection _serviceConnection;
        private string _publickey = "Include your Public Key using the Google Play Console for Developers"; //Insert here your Public Key

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);            

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button btnShowItems = FindViewById<Button>(Resource.Id.btnShowItems);
            btnShowItems.Click += BtnShowItems_Click;

            Button btnBuyFake = FindViewById<Button>(Resource.Id.btnBuyFake);
            btnBuyFake.Click += BtnBuyFake_Click;

            Button btnBuyLevel1 = FindViewById<Button>(Resource.Id.btnBuyLevel1);
            btnBuyLevel1.Click += BtnBuyLevel1_Click;

            try
            {
                // Create a new connection to the Google Play Service
                _serviceConnection = new InAppBillingServiceConnection(this, _publickey);
                _serviceConnection.OnConnected += _serviceConnection_OnConnected;

                // Attempt to connect to the service
                _serviceConnection.Connect();
            }
            catch (Exception exc)
            {
                throw exc;
            }
            //Remember to unbind from the In-app Billing service when you are done with your Activity
        }

        private void BtnBuyLevel1_Click(object sender, EventArgs e)
        {
            if (_products[4].ProductId == "level1")
            {
                _serviceConnection.BillingHandler.BuyProduct(_products[4]);
            }
            else
            {
                Toast.MakeText(this, "Product ID not available", ToastLength.Long).Show();
            }
        }

        private void BtnBuyFake_Click(object sender, EventArgs e)
        {
            // Buy the fake product with no cost

            if (_products[2].ProductId == "android.test.purchased")
            {
                _serviceConnection.BillingHandler.BuyProduct(_products[2]);
            }
            else
            {
                Toast.MakeText(this, "Product ID not available", ToastLength.Long).Show();
            }
        }

        private void BtnShowItems_Click(object sender, EventArgs e)
        {
            // Ask the open connection's billing handler to get any purchases
            var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);

            string myPurchases = string.Empty;

            for(int i = 0; i<purchases.Count; i++)
            {
                myPurchases += purchases[i].ProductId;
            }

            Toast.MakeText(this, "You own: " + myPurchases, ToastLength.Long).Show();
        }

        private async void _serviceConnection_OnConnected()
        {
            try
            {
                //Adding "level1" since it is defined this way on the in the product list of the Google Console Play of this app

                _products = await _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
                ReservedTestProductIDs.Purchased,
                ReservedTestProductIDs.Canceled,
                ReservedTestProductIDs.Refunded,
                ReservedTestProductIDs.Unavailable,
                "level1"
               }, ItemType.Product);

                // Were any products returned?
                if (_products == null)
                {
                    // No, abort
                    return;
                }                
            }
            catch (Exception exc)
            {
                string error = exc.Message;
                throw exc;
            }         
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Ask the open service connection's billing handler to process this request
            _serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
            // TODO: Use a call back to update the purchased items
            // or listen to the OnProductPurchased event to
            // handle a successful purchase
        }

        protected override void OnDestroy()
        {
            // Are we attached to the Google Play Service?
            if (_serviceConnection != null)
            {
                // Yes, disconnect
                _serviceConnection.Disconnect();
            }
            // Call base method
            base.OnDestroy();
        }
    }
}

