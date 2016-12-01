using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Debug = System.Diagnostics.Debug;
namespace ProcessDashboard.Droid.Fragments
{
    public class DatePickerFragment : DialogFragment,DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        public string positiveText = "Ok";

        public string negativeText = "Cancel";

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> _dateSelectedHandler = delegate { };

        public bool DateTimePicker { get; set; }

        public DateTime StartTime { get; set; }

        public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected)
        {
            DatePickerFragment frag = new DatePickerFragment();
            //frag.Theme = Android.Resource.Style.ThemeDialog;

            frag._dateSelectedHandler = onDateSelected;
            return frag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            
            View v = inflater.Inflate(Resource.Layout.DateTimePickerDialogFragment, container);
            //v.FindView
            DatePicker dp = (DatePicker)v.FindViewWithTag("customdatepicker");
            Button okButton = (Button)v.FindViewById(Resource.Id.positiveButton);

            okButton.Click += (sender, args) =>
            {
                OnDateSet(dp, dp.Year, dp.Month, dp.DayOfMonth);
                this.Dismiss();
            };


            Button cancelButton = (Button)v.FindViewById(Resource.Id.negativeButton);
            cancelButton.Click += (sender, args) =>
            {
                this.Dismiss();
            };

            return v;
            
            //return inflater.Inflate(Resource.Layout.DateTimePickerDialogFragment,container);
        }
        
        /*
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently = StartTime;
            
               
            DatePickerDialog dialog = new DatePickerDialog(Activity,Android.Resource.Style.ThemeHoloDialog,
                                                           this,
                                                           currently.Year,
                                                           currently.Month-1,
                                                           currently.Day);

            


            TimeSpan t = DateTime.Now - new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Local);

            dialog.DatePicker.CalendarViewShown = false;
            dialog.DatePicker.SpinnersShown = true;

            dialog.DatePicker.MaxDate = (long)t.TotalMilliseconds;

            dialog.SetButton((int)DialogButtonType.Positive,positiveText,dialog);
            z
            //dialog.GetButton((int) Android.Content.DialogButtonType.Positive).SetText(positiveText,TextView.BufferType.Normal);
            //dialog.GetButton((int)Android.Content.DialogButtonType.Negative).SetText(negativeText, TextView.BufferType.Normal);
            
            return dialog;
        }
        */
        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            Debug.WriteLine("Date Selected is :" + selectedDate.ToShortDateString());
            StartTime = selectedDate;
            //Log.Debug(TAG, selectedDate.ToLongDateString());
            _dateSelectedHandler(selectedDate);
        }

        

    }

    public class TimePickerFragment : DialogFragment,
                                  TimePickerDialog.IOnTimeSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<int,int> _timeSelectedHandler = delegate { };

        public bool DateTimePicker { get; set; }

        public int StartHour { get; set; }

        public int StartMinute { get; set; }


        public DateTime chosenDate { get; set; }

        public static TimePickerFragment NewInstance(Action<int,int> onTimeSelected)
        {
            TimePickerFragment frag = new TimePickerFragment();
            frag._timeSelectedHandler = onTimeSelected;

       // frag.

            return frag;
        }




        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            TimePickerDialog dialog = new TimePickerDialog(Activity, this, StartHour, StartMinute, true);
            return dialog;
        }

      
        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            Debug.WriteLine("Hour of day :" + hourOfDay + " Minute :" + minute);
            
            
            StartHour = hourOfDay;
            StartMinute = minute;

            if (chosenDate.Date.Equals(DateTime.Now.Date))
            {
                if(chosenDate.Hour<StartHour)
                {
                    Toast.MakeText(Activity, "Please choose a valid time", ToastLength.Long).Show();
                }
                else if (chosenDate.Hour == StartHour && chosenDate.Minute < StartMinute)
                {
                    Toast.MakeText(Activity, "Please choose a valid time", ToastLength.Long).Show();
                }
                else
                    _timeSelectedHandler(hourOfDay, minute);
            }
            else
            { 
                _timeSelectedHandler(hourOfDay, minute);
            }
        }
    }
}