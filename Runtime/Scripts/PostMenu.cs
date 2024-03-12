using System;
using System.Collections.Generic;
using BRIJ.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BRIJ
{
    public class PostMenu : MonoBehaviour
    {
        public BrijMain brij;
        public RawImage image;
        public TMP_Text userKarmaText;
        public TMP_Text usernameText;
        public TMP_Text experienceText;
        public GameObject emojiListObj;
        public GameObject emojiPrefab;

        public GameObject emojiExpandIcon;
        public GameObject emojiCollapseIcon;

        private Dictionary<string, EmojiElement> _emojiElements = new Dictionary<string, EmojiElement>();
        private PostData _postData = null;
        private bool _emojiExpanded = false;

        public void InitEmoji()
        {
            foreach (EmojiDef emojiDef in brij.emoji)
            {
                GameObject emojiObj = Instantiate(emojiPrefab, emojiListObj.transform);
                EmojiElement emojiElement = emojiObj.GetComponent<EmojiElement>();
                emojiElement.brij = brij;
                emojiElement.Name = emojiDef.name;
                emojiElement.image.sprite = emojiDef.sprite;
                emojiElement.countText.text = "0";
                _emojiElements.Add(emojiDef.name, emojiElement);
            }
            
            emojiListObj.gameObject.SetActive(false);
            emojiExpandIcon.gameObject.SetActive(true);
            emojiCollapseIcon.gameObject.SetActive(false);
        }

        public void OnClose()
        {
            gameObject.SetActive(false);
        }

        public void SetPost(PostData postData)
        {
            _postData = postData;
            
            image.texture = postData.PostImg;
            usernameText.text = postData.Post.user.username;
            userKarmaText.text = postData.Post.user.points.ToString();
            experienceText.text = postData.Post.experienceName;

            UpdateReactions();
        }

        public void UpdateReactions()
        {
            foreach (var pair in _emojiElements)
            {
                pair.Value.countText.text = "0";
                pair.Value.Post = _postData.Post;
                pair.Value.Reaction = null;
                pair.Value.Menu = this;
            }
            
            foreach (PostReactionModel reaction in _postData.Post.reactions)
            {
                if (_emojiElements.TryGetValue(reaction.name, out EmojiElement emojiElement))
                {
                    emojiElement.countText.text = reaction.count.ToString();
                    emojiElement.Post = _postData.Post;
                    emojiElement.Reaction = reaction;
                    emojiElement.Menu = this;
                }
            }
        }

        public bool RemoveUserReaction()
        {
            foreach (var pair in _emojiElements)
            {
                if (pair.Value.Reaction != null && pair.Value.Reaction.reacted)
                {
                    pair.Value.Reaction.reacted = false;
                    pair.Value.Reaction.count--;
                    return true;
                }
            }

            return false;
        }

        public void EmojiOpenClosePressed()
        {
            _emojiExpanded = !_emojiExpanded;
            
            emojiExpandIcon.gameObject.SetActive(!_emojiExpanded);
            emojiCollapseIcon.gameObject.SetActive(_emojiExpanded);
            emojiListObj.gameObject.SetActive(_emojiExpanded);
        }

        public void GoToExperiencePressed()
        {
            if (_postData.Post.experienceUrl.Length > 0)
            {
                Application.OpenURL(_postData.Post.experienceUrl);
            }
        }
    }
}