using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class ObjectDestroyer : NetworkBehaviour
    {
        [SerializeField] private DestroyBehaviourContainer[] affectedBehaviours;
        [SerializeField] private DestroyGameObjectContainer[] affectedObjects;


        public override void OnNetworkSpawn()
        {
            int affectedBehavioursCount = affectedBehaviours.Length;
            for (int i = 0; i < affectedBehavioursCount; i++)
            {
                affectedBehaviours[i].Execute(IsOwner);
            }

            int affectedObjectsCount = affectedObjects.Length;
            for (int i = 0; i < affectedObjectsCount; i++)
            {
                affectedObjects[i].Execute(IsOwner);
            }

            Destroy(this);
        }


        [System.Serializable]
        public class DestroyBehaviourContainer
        {
            public Component component;
            public DestroyOptionMode action;
            public OwnerType target;


            public void Execute(bool calledForOwner)
            {
                if (target == (OwnerType)(calledForOwner ? 1 : 0)) return;

                switch (action)
                {
                    case DestroyOptionMode.Destroy:

                        Destroy(component);
                        break;

                    case DestroyOptionMode.Disable:

                        if (((Behaviour)component) == null) return;

                        ((Behaviour)component).enabled = false;
                        break;

                    default:
                        break;
                }
            }
        }

        [System.Serializable]
        public class DestroyGameObjectContainer
        {
            public GameObject obj;
            public DestroyOptionMode action;
            public OwnerType target;


            public void Execute(bool calledForOwner)
            {
                if (target == (OwnerType)(calledForOwner ? 1 : 0)) return;

                switch (action)
                {
                    case DestroyOptionMode.Destroy:

                        Destroy(obj);
                        break;

                    case DestroyOptionMode.Disable:

                        obj.SetActive(false);
                        break;

                    default:
                        break;
                }
            }
        }

        public enum DestroyOptionMode : byte
        {
            Destroy,
            Disable
        }
        public enum OwnerType : byte
        {
            Owner,
            NonOwner
        }
    }
}