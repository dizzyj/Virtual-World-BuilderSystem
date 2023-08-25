using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerInstanceManager : NetworkBehaviour
{
    private CVWNetworkManager manager;
    private NetworkIdentity selfPlayer;
    public string currentSceneName;

    // Start is called before the first frame update
    void Start()
    {
        manager = CVWNetworkManager.singleton;
        selfPlayer = NetworkClient.localPlayer;
    }

    #region Public Methods

        // Move the player to the waiting room
        public void MoveToWaitingRoom() {
            if (!isLocalPlayer) return;

            if (manager.mode == NetworkManagerMode.ClientOnly) {
                // Unload existing instances
                StartCoroutine(UnloadNonContainerScenes());
                CmdMoveToWaitingRoom();
            }

            else if (manager.mode == NetworkManagerMode.Host) {
                CmdMoveToWaitingRoom();
            }
        }
        
        // Create an office for the player and move to the office
        public void CreateAndMoveToOffice() {
            if (!isLocalPlayer) return;

            if (manager.mode == NetworkManagerMode.ClientOnly) {
                // Unload existing instances
                StartCoroutine(UnloadNonContainerScenes());
                CmdCreateAndMoveToOffice();
            }

            else if (manager.mode == NetworkManagerMode.Host) {
                CmdCreateAndMoveToOffice();
            }
        }

        public void MoveToStartPosition() {
            StartCoroutine(MoveToStartPositionDelayed());
        }

        IEnumerator MoveToStartPositionDelayed() {
            while (NetworkClient.isLoadingScene)
                yield return null;

            Transform targetTransform = manager.GetStartingPositionForScene(currentSceneName);
            MovePlayerTransformTo(targetTransform);
        }

        [TargetRpc]
        public void TargetMoveToStartPosition(NetworkConnection target) {
            MoveToStartPosition();
        }

        public void MovePlayerToMyScene(uint targetId) {
            CmdMovePlayerToMyScene(targetId, selfPlayer);
        }

        // Get players current scene location
        public string GetCurrentSceneName() {
            return currentSceneName;
        }

        public void SetCurrentSceneName(string newSceneName) {
            currentSceneName = newSceneName;
        }

        [TargetRpc]
        public void TargetSetCurrentSceneName(NetworkConnection conn, string newSceneName) {
            SetCurrentSceneName(newSceneName);
        }
        
    #endregion
    
    #region Commands

        // Move player to waiting room on server
        [Command]
        public void CmdMoveToWaitingRoom() {
            manager.ServerMovePlayerToWaitingRoom(gameObject);
        }

        // Create office for player and move on server
        [Command]
        public void CmdCreateAndMoveToOffice() {
            manager.ServerCreateOfficeAndMovePlayer(gameObject);
        }

        [Command(requiresAuthority=false)]
        public void CmdMovePlayerToMyScene(uint playerId, NetworkIdentity self) {
            NetworkIdentity targetIdentity = NetworkServer.spawned[playerId];
            manager.ServerMovePlayerToPlayerScene(targetIdentity, self);
            // Transform targetTransform = manager.GetStartingPositionForMyScene();
            // TargetMoveToStartPosition(player.GetComponent<NetworkIdentity>().connectionToClient, targetTransform);
        }

        [Command(requiresAuthority=false)]
        public void CmdMoveToStartPosition() {
            Transform targetTransform = manager.GetStartingPositionForScene(currentSceneName);
            MovePlayerTransformTo(targetTransform);
        }


    #endregion

    #region Helper functions

        // Unload all but the container scene
        IEnumerator UnloadNonContainerScenes() {
            for (int index = 1; index < SceneManager.sceneCount; index++)
            {
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
            }

        }

        // Helper function
        private void MovePlayerTransformTo(Transform target) {
            if (target == null) return;
            transform.position = target.position;
            transform.rotation = target.rotation;
        }

        // public void MoveToStartPositionAfterSceneLoad() {
        //     StartCoroutine(WaitOnSceneLoadThenMoveToStartPosition());
        // }

        // IEnumerator WaitOnSceneLoadThenMoveToStartPosition() {
        //     while (!currentSceneName.Equals(gameObject.scene.path)) {
        //         yield return null;
        //     }

        //     Debug.Log(currentSceneName + "\n" + gameObject.scene.path);
        //     MoveToStartPosition();
        // }

    #endregion
}
