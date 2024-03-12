using System.Collections.Generic;
using BRIJ.Code.Models;
using BRIJ.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Application = UnityEngine.Application;

#if PLATFORM_ANDROID && PLATFORM_OCULUS
using System;
using Oculus.Platform;
using Oculus.Platform.Models;
#endif

namespace BRIJ
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(TrackedDeviceGraphicRaycaster))]
    public class BrijMain : MonoBehaviour
    {
        [Header("Dev settings")]
        public string gameToken;

        public bool loginOnStart;
        public bool rememberLogin = true;
        public bool loginWithMetaAccount = true;
        
        public PhotoCamera handHeldCamera = null;

        [Header("Desktop mode")]
        public bool desktopMode = false;
        public GameObject desktopSystem;
        public GameObject vrRig;
        public Camera desktopCamera;
        
        [Header("Emoji images")]
        public List<EmojiDef> emoji;

        [Header("Private fields")]
        public LoginMenu loginMenu;
        public MomentsMenu momentMenu;
        public PostMenu postMenu;
        public CameraMenu cameraMenu;

        public GameObject usernameLabelObj;
        public TMP_Text usernameTextLabel;
        public TMP_Text userPointsLabel;

        public GameObject momentsButtonObj;
        public GameObject cameraButtonObj;

        #region PrivateFields

        private GameSessionModel _sessionModel;
        private Dictionary<string, Sprite> _emojiSprites;
        private Dictionary<long, PostData> _posts;
        private bool _loadingPosts = false;
        
        private ulong _userId;

        #endregion

        private void Start()
        {
            if (desktopMode)
            {
                GetComponent<GraphicRaycaster>().enabled = desktopMode;
                GetComponent<TrackedDeviceGraphicRaycaster>().enabled = !desktopMode;
                desktopSystem.SetActive(desktopMode);
                vrRig.SetActive(!desktopMode);

                Canvas canvas = GetComponent<Canvas>();
                canvas.renderMode = desktopMode ? RenderMode.ScreenSpaceCamera : RenderMode.WorldSpace;
                canvas.worldCamera = desktopMode ? desktopCamera : null;
            }
            
            if (loginOnStart)
            {
                BeginLogin();
            }

            _emojiSprites = new Dictionary<string, Sprite>();
            foreach (EmojiDef emojiDef in emoji)
            {
                _emojiSprites.Add(emojiDef.name, emojiDef.sprite);
            }

            _posts = new Dictionary<long, PostData>();

            postMenu.InitEmoji();
        }
#if PLATFORM_ANDROID && PLATFORM_OCULUS
        private void OnInitializationCallback(Message<PlatformInitialize> msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error during initialization. Error Message: {0}",
                    msg.GetError().Message);
                loginMenu.gameObject.SetActive(true);
            }
            else
            {
                Entitlements.IsUserEntitledToApplication().OnComplete(OnIsEntitledCallback);
            }
        }
        
        private void OnIsEntitledCallback(Message msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error verifying the user is entitled to the application. Error Message: {0}",
                    msg.GetError().Message);
                loginMenu.gameObject.SetActive(true);
            }
            else
            {
                GetLoggedInUser();
            }
        }
        
        private void GetLoggedInUser()
        {
            Users.GetLoggedInUser().OnComplete(OnLoggedInUserCallback);
        }

        private void OnLoggedInUserCallback(Message<User> msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error getting logged in user. Error Message: {0}",
                    msg.GetError().Message);
                loginMenu.gameObject.SetActive(true);
            }
            else
            {
                _userId = msg.Data.ID; // do not use msg.Data.OculusID;
                Debug.Log("UserId: " + _userId.ToString());
                GetUserProof();
            }
        }
        
        private void GetUserProof()
        {
            Users.GetUserProof().OnComplete(OnUserProofCallback);
        }

        private void OnUserProofCallback(Message<UserProof> msg)
        {
            if (msg.IsError)
            {
                Debug.LogErrorFormat("Oculus: Error getting user proof. Error Message: {0}",
                    msg.GetError().Message);
                loginMenu.gameObject.SetActive(true);
            }
            else
            {
                string oculusNonce = msg.Data.Value;
                Debug.Log("Oculus nonce: " + oculusNonce);
                // Authentication can be performed here
                
                StartCoroutine(Api.LoginWithMeta(oculusNonce, _userId, gameToken, session =>
                {
                    Login(session);
                }, errorInfo =>
                {
                    Debug.Log("Could not login with meta ! (" + errorInfo + ")");
                    loginMenu.gameObject.SetActive(true);
                }));
            }
        }
#endif
        private void BeginLogin()
        {
            if (rememberLogin)
            {
                string sessionToken = PlayerPrefs.GetString("BRIJ_SessionToken", "");

                if (sessionToken.Length > 0)
                {
                    StartCoroutine(Api.RestoreSession(sessionToken, model =>
                    {
                        if (model.successful)
                        {
                            GameSessionModel sessionModel = new GameSessionModel();
                            sessionModel.sessionToken = sessionToken;
                            sessionModel.user = model.user;
                            sessionModel.successful = true;
                            Login(sessionModel);
                        }
                        else
                        {
#if PLATFORM_ANDROID && PLATFORM_OCULUS
                            if (loginWithMetaAccount)
                            {
                                if (Application.platform == RuntimePlatform.Android)
                                {
                                    Core.AsyncInitialize().OnComplete(OnInitializationCallback);
                                    return;
                                }
                            }
#endif
                            
                            loginMenu.gameObject.SetActive(true);
                        }
                    }));
                    return;
                }
            }
            else
            {
#if PLATFORM_ANDROID && PLATFORM_OCULUS
                if (loginWithMetaAccount)
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        Core.AsyncInitialize().OnComplete(OnInitializationCallback);
                        return;
                    }
                }
#endif
            }
            
            loginMenu.gameObject.SetActive(true);
        }

        private void Update()
        {
            
        }

        public void LoadPosts()
        {
            if (_loadingPosts)
                return;
            
            _loadingPosts = true;
            
            // TODO: Implement auto loading posts
            StartCoroutine(Api.GetPosts(_sessionModel, 0, 0, 0, OnPostLoaded, OnPostError));
        }

        private void OnPostLoaded(List<PostModel> posts)
        {
            _loadingPosts = false;
            momentMenu.DisableLoading();
            
            foreach (PostModel post in posts)
            {
                PostData postData;
                if (_posts.TryGetValue(post.id, out postData))
                {
                    // TODO: Update the moment and post if opened
                }
                else
                {
                    postData = new PostData();
                    postData.Post = post;
                    _posts.Add(post.id, postData);
                    PostMoment moment = momentMenu.AddPost(postData);
                    StartCoroutine(Api.GetImage(post.image, texture =>
                    {
                        postData.PostImg = texture;
                        moment.image.texture = postData.PostImg;
                    }));
                }
            }
        }

        private void OnPostError(string error)
        {
            _loadingPosts = false;
            momentMenu.DisableLoading();
            Debug.Log(error);
        }

        public void Login(GameSessionModel session)
        {
            _sessionModel = session;
            
            usernameLabelObj.gameObject.SetActive(true);
            usernameTextLabel.text = session.user.username;
            userPointsLabel.text = session.user.points.ToString();
            
            cameraButtonObj.SetActive(true);
            
            PlayerPrefs.SetString("BRIJ_SessionToken", session.sessionToken);
            PlayerPrefs.Save();
            OpenMomentsMenu();
        }

        public void ClearPosts()
        {
            _posts.Clear();
            momentMenu.ClearPosts();
        }

        public void OpenMomentsMenu()
        {
            momentMenu.gameObject.SetActive(true);
            momentsButtonObj.SetActive(false);
            // For now just clear posts and load them again
            _posts.Clear();
            LoadPosts();
        }

        public void CloseMomentsMenu()
        {
            momentMenu.ClearPosts();
            momentMenu.loadingIconObj.gameObject.SetActive(true);
            momentMenu.gameObject.SetActive(false);
            momentsButtonObj.SetActive(true);
        }

        public GameSessionModel GetSession()
        {
            return _sessionModel;
        }

        public bool TryGetEmoji(string emojiName, out Sprite sprite)
        {
            return _emojiSprites.TryGetValue(emojiName, out sprite);
        }

        public void OnEmojiClick(EmojiElement element)
        {
            Debug.Log("Clicked emoji: " + element.Name);

            bool removed = element.Menu.RemoveUserReaction();
            
            if (element.Reaction == null)
            {
                element.Post.reactions.Add(new PostReactionModel(element.Name, 1, true));
            }
            else
            {
                if (!removed)
                {
                    element.Reaction.reacted = true;
                    element.Reaction.count++;
                }
            }
            
            element.Menu.UpdateReactions();

            StartCoroutine(Api.ReactOnPost(_sessionModel.sessionToken, element.Post.id, element.Name, removed));
        }

        public void OpenPost(PostData postData)
        {
            cameraMenu.gameObject.SetActive(false);
            postMenu.gameObject.SetActive(true);
            postMenu.SetPost(postData);
        }

        public void OpenCamera()
        {
            postMenu.gameObject.SetActive(false);
            cameraMenu.gameObject.SetActive(true);
        }

        public void CloseCamera()
        {
            cameraMenu.Close();
            cameraMenu.gameObject.SetActive(false);
        }
    }
}