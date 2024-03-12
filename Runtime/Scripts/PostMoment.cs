using System;
using BRIJ.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BRIJ
{
    public class PostMoment : MonoBehaviour
    {
        public RawImage image;
        public BrijMain brij;

        [Header("ReactionDisplay")]
        public GameObject reactionDisplay;
        public GameObject highestEmoji1;
        public GameObject highestEmoji2;
        public Image highestEmoji1Image;
        public Image highestEmoji2Image;
        public TMP_Text text;

        private PostData _postData;

        public PostData PostData
        {
            get => _postData;
            set => _postData = value;
        }

        public void OnMomentClicked()
        {
            brij.OpenPost(PostData);
        }
        
        public void OnGoClicked()
        {
            if (PostData.Post.experienceUrl.Length > 0)
            {
                Application.OpenURL(PostData.Post.experienceUrl);
            }
        }
    }
}