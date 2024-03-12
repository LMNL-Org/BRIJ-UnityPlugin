using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BRIJ.Models;
using TMPro;
using UnityEngine;

namespace BRIJ
{
    public class MomentsMenu : MonoBehaviour
    {
        public BrijMain brij;
        public GameObject contentObj;
        public GameObject momentPrefab;
        public GameObject loadingIconObj;

        public void ClearPosts()
        {
            int childCount = contentObj.transform.childCount;

            for (int i = childCount - 1; i > 0; i--)
            {
                GameObject.Destroy(contentObj.transform.GetChild(i).gameObject);
            }
        }

        public void DisableLoading()
        {
            loadingIconObj.gameObject.SetActive(false);
        }

        public PostMoment AddPost(PostData postData)
        {
            GameObject momentObj = Instantiate(momentPrefab, contentObj.transform);
            PostMoment moment = momentObj.GetComponent<PostMoment>();
            moment.PostData = postData;
            moment.brij = brij;

            PostModel post = postData.Post;
            
            if (post.reactions.Count > 0)
            {
                moment.reactionDisplay.gameObject.SetActive(true);
                post.reactions.Sort();

                for (int i = 0; i < Math.Min(post.reactions.Count(), 2); i++)
                {
                    PostReactionModel reactionModel = post.reactions[i];

                    if (i == 0)
                    {
                        moment.highestEmoji1.gameObject.SetActive(true);
                        brij.TryGetEmoji(reactionModel.name, out Sprite emoji);
                        moment.highestEmoji1Image.sprite = emoji;
                    }
                    else
                    {
                        moment.highestEmoji2.gameObject.SetActive(true);
                        brij.TryGetEmoji(reactionModel.name, out Sprite emoji);
                        moment.highestEmoji2Image.sprite = emoji;
                    }
                }

                if (post.reactions.Count > 2)
                {
                    moment.text.text = "+" + (post.reactions.Count - 2);
                    moment.text.gameObject.SetActive(true);
                }
            }

            return moment;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}