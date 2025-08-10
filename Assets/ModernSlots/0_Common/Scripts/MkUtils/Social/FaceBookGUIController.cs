using UnityEngine;
using UnityEngine.UI;

/*
    23.10.2019 - first
    15.11.2019 - add photo behavior
    28.02.2020 - add strings connect/ disconnect
    30.06.2020 - remove player reference
    15.10.2020 - add fb name;
    08.12.2020 - unlink photo from avatar group
 */
namespace Mkey
{
    public class FaceBookGUIController : MonoBehaviour
    {
        [SerializeField]
        private Text loginText;
        [SerializeField]
        private string connectString;
        [SerializeField]
        private string disConnectString;
        [SerializeField]
        private Image fbPhoto;
        [SerializeField]
        private Text fbName;
        [SerializeField]
        private GameObject avatarGroup;

        #region temp vars
        private FBholder FB => FBholder.Instance;
        private Sprite defaultPlayerImage;
        #endregion temp vars

        #region regular
        private void Start()
        {
            if (fbPhoto) defaultPlayerImage = fbPhoto.sprite;

            FBholder.LoginEvent += FacebooLoginHandler;
            FBholder.LogoutEvent += FacebooLogoutHandler;
            FBholder.LoadPhotoEvent += LoadPhotoEventHandler;
            FBholder.LoadTextEvent += LoadTextEventHandler;
            Refresh();
        }

        private void OnDestroy()
        {
            FBholder.LoginEvent -= FacebooLoginHandler;
            FBholder.LogoutEvent -= FacebooLogoutHandler;
            FBholder.LoadPhotoEvent -= LoadPhotoEventHandler;
        }
        #endregion regular

        private void Refresh()
        {
            if (loginText) loginText.text = (!FBholder.IsLogined) ? connectString : disConnectString;
            if (fbName && FB) fbName.text = (!FBholder.IsLogined) ? "" : FB.playerFirstName;

            if (fbPhoto && FB && FB.playerPhoto) fbPhoto.sprite = (!FBholder.IsLogined) ? defaultPlayerImage : FB.playerPhoto;

            if (avatarGroup && fbPhoto && FB)
            {
                avatarGroup.SetActive(FBholder.IsLogined && FB.playerPhoto);
            }
            else if (avatarGroup)
            {
                avatarGroup.SetActive(false);
            }
        }

        public void FaceBook_Click()
        {
            if (!FB) return;
            if (FBholder.IsLogined)
            {
                FB.FBlogOut();
            }
            else
            {
                FB.FBlogin();
            }
        }

        #region event handlers
        private void FacebooLoginHandler(bool logined, string message)
        {
            Refresh();
        }

        private void FacebooLogoutHandler()
        {
            Refresh();
        }

        private void LoadPhotoEventHandler(bool isLogined, Sprite photo)
        {
            if (fbPhoto && photo) fbPhoto.sprite = photo;

            if (avatarGroup && fbPhoto)
            {
                avatarGroup.SetActive(FBholder.IsLogined && photo);
            }
            else if (avatarGroup)
            {
                avatarGroup.SetActive(false);
            }
        }

        private void LoadTextEventHandler(bool isLogined, string firstName, string lastName) // logined, first name, last name
        {
            if (fbName && FB) fbName.text = (!isLogined) ? "" : firstName;
        }
        #endregion event handlers
    }
}
