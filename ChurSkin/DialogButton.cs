using System.Globalization;

namespace System.Windows.Forms
{
    internal static class DialogButton
    {
        internal static readonly string BtnAbort;
        internal static readonly string BtnCancel;
        internal static readonly string BtnIgnore;
        internal static readonly string BtnNo;
        internal static readonly string BtnOk;
        internal static readonly string BtnRetry;
        internal static readonly string BtnYes;

        static DialogButton()
        {
            if (lang == "zh-CN")
            {
                BtnCancel = "ȡ��";
                BtnIgnore = "����";
                BtnNo = "��";
                BtnOk = "ȷ��";
                BtnRetry = "����";
                BtnYes = "��";
                BtnAbort = "��ֹ";
            }
            else if (lang == "en-US" || lang == "en-NG")
            {
                BtnCancel = "Cancel";
                BtnIgnore = "Ignore";
                BtnNo = "No";
                BtnOk = "Ok";
                BtnRetry = "Retry";
                BtnYes = "Yes";
                BtnAbort = "Aborted";
            }
            else if (lang == "zh-HK" || lang == "zh-TW")
            {
                BtnCancel = "ȡ��";
                BtnIgnore = "����";
                BtnNo = "��";
                BtnOk = "�_��";
                BtnRetry = "��ԇ";
                BtnYes = "��";
                BtnAbort = "��ֹ";
            }
        }

        internal static string lang
        {
            get { return CultureInfo.InstalledUICulture.Name; }
        }
    }
}