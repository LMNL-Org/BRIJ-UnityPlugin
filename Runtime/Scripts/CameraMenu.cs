using System;
using UnityEngine;

namespace BRIJ
{
    [RequireComponent(typeof(AudioSource))]
    public class CameraMenu : MonoBehaviour
    {
        public BrijMain brij;
        public PhotoCamera mirrorCamera;
        public GameObject mirrorPlaceholder;

        private bool MirrorCameraOpened = false;
        private AudioSource SnapAudioSource;

        private void Start()
        {
            SnapAudioSource = GetComponent<AudioSource>();
        }

        public void FlipCamera()
        {
            MirrorCameraOpened = !MirrorCameraOpened;
            
            mirrorCamera.gameObject.SetActive(MirrorCameraOpened);
            mirrorPlaceholder.gameObject.SetActive(!MirrorCameraOpened);
            
            if (brij.handHeldCamera)
                brij.handHeldCamera.gameObject.SetActive(!MirrorCameraOpened);
        }

        private void OnEnable()
        {
            if (brij.desktopMode)
                MirrorCameraOpened = true;
            
            mirrorCamera.gameObject.SetActive(MirrorCameraOpened);
            mirrorPlaceholder.gameObject.SetActive(!MirrorCameraOpened);
            brij.handHeldCamera.gameObject.SetActive(!MirrorCameraOpened);
        }

        public void Close()
        {
            mirrorCamera.gameObject.SetActive(true);
            mirrorPlaceholder.gameObject.SetActive(false);
            if (brij.handHeldCamera)
                brij.handHeldCamera.gameObject.SetActive(false);
        }

        public void TakePicture()
        {
            RenderTexture renderTexture = MirrorCameraOpened || (brij.handHeldCamera == null) ? mirrorCamera.renderTexture : brij.handHeldCamera.renderTexture;

            if (renderTexture == null)
            {
                Debug.LogError("Render target texture is null!");
                return;
            }
            
            Texture2D temp = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            var oldRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            temp.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0, false);
            temp.Apply();
            RenderTexture.active = oldRT;

            byte[] png = temp.EncodeToPNG();
            StartCoroutine(Api.UploadPost(brij.GetSession().sessionToken, png, OnPostUploaded));
        }

        private void OnPostUploaded(bool success, string info)
        {
            if (SnapAudioSource)
                SnapAudioSource.Play();

            brij.ClearPosts();
            brij.LoadPosts();
            brij.CloseCamera();
        }
    }
}