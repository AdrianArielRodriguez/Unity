using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;
// using Kinect = Windows.Kinect;

public class AvatarControl : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mAvatar; //agrega el prefabs DESDE LA ESCENA de avatar 
    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>(); //es una lista de los cuerpos que la camara puede seguir
    protected Transform[] bones;

    void Update()
    {
        #region Get Kinect data

        Body[] data = mBodySourceManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null) continue;
            if (body.IsTracked) trackedIds.Add(body.TrackingId);
        }
        #endregion

        bones = new Transform[_BoneMap.Count];

        #region Delate Kinect bodies

        List<ulong> knownIds = new List<ulong>(mBodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(mBodies[trackingId]);
                mBodies.Remove(trackingId);
            }
        }
        #endregion

        #region Create Kinect bodies
        foreach (var body in data)
        {
            if (body == null) continue;

            if (body.IsTracked)
            {
                if (!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }
        }
        #endregion
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        body = Instantiate(mAvatar);

        var animatorComponent = mAvatar.GetComponent<Animator>();

        for (int boneIndex = 0; boneIndex < _BoneMap.Count; boneIndex++)
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue;

            bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]); //GetBone es un metodo propio de Animator sige una lista especifica llamada HumanBodyBones
            bones[boneIndex].name = _BoneMap[boneIndex].ToString();
            bones[boneIndex].transform.rotation = Quaternion.identity;
            bones[boneIndex].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            bones[boneIndex].transform.parent = body.transform;
            
        }
        //lo devuelve sin cargar el transform de las juntas
        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        //foreach (JointType _joint in _joints)//_joint es un tipo de lista con <JointType, Joint> con un solo elemento, pero _joints es una lista "<JointType, Joint>"

        for (int boneIndex = 0; boneIndex < _BoneMap.Count; boneIndex++)
        {
            JointType jt = _BoneMap[boneIndex];
            Joint sourceJoint = body.Joints[jt];//por cada articulacion le da las posiciones de la fuente
            //body.Joints[] es una propiedad que necesita de una lista <JointType, Joint> para devolver los datos
            //Joint se forma por el nombre, la posiscion y el estado
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);//transforma los valores de la fuente en vectores de Unity

            JointOrientation sourceJointO = body.JointOrientations[jt];
            Quaternion targetOrientation = GetQuaternion3FromJoint(sourceJointO);

            Transform jointObject = bodyObject.transform.Find(jt.ToString());
            //del GameObject que se le pasa
            //busca a las juntas hijas con el mismo id, asocia el movimiento con el objeto ya creado
            //crea el transform de las juntas
            jointObject.position = targetPosition; //actualiza el transform de las juntas con el movimiento actual
            jointObject.rotation = targetOrientation;
        }
    }

    private static Vector3 GetVector3FromJoint(Joint joint)
    {
        int FA = 10;//Factor de amplificacion que amplifica los valores de la fuente
        return new Vector3(joint.Position.X * FA, joint.Position.Y * FA, joint.Position.Z * FA);
    }

    private static Quaternion GetQuaternion3FromJoint(JointOrientation joint)
    {
        return new Quaternion(joint.Orientation.X, joint.Orientation.Y, joint.Orientation.Z, joint.Orientation.X);
    }

    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
    {
        {0, HumanBodyBones.Hips},
        {1, HumanBodyBones.Spine},
        {2, HumanBodyBones.Neck},
        {3, HumanBodyBones.Head},
        {4, HumanBodyBones.LeftUpperArm},
        {5, HumanBodyBones.LeftLowerArm},
        {6, HumanBodyBones.LeftHand},
        {7, HumanBodyBones.LeftThumbProximal},
        {8, HumanBodyBones.RightUpperArm},
        {9, HumanBodyBones.RightLowerArm},
        {10, HumanBodyBones.RightHand},
        {11, HumanBodyBones.RightThumbProximal},
        {12, HumanBodyBones.LeftUpperLeg},
        {13, HumanBodyBones.LeftLowerLeg},
        {14, HumanBodyBones.LeftFoot},
        {15, HumanBodyBones.LeftToes},
        {16, HumanBodyBones.RightUpperLeg},
        {17, HumanBodyBones.RightLowerLeg},
        {18, HumanBodyBones.RightFoot},
        {19, HumanBodyBones.RightToes},
        {20, HumanBodyBones.Chest},
        {21, HumanBodyBones.LeftIndexProximal},
        {22, HumanBodyBones.LeftThumbIntermediate},
        {23, HumanBodyBones.RightIndexProximal},
        {24, HumanBodyBones.RightThumbIntermediate},
    };

    private readonly Dictionary<int,JointType> _BoneMap = new Dictionary<int, JointType>()
    {
        {0, JointType.SpineBase},
        {1, JointType.SpineMid},
        {2, JointType.Neck},
        {3, JointType.Head},
        {4, JointType.ShoulderLeft},
        {5, JointType.ElbowLeft},
        {6, JointType.WristLeft},
        {7, JointType.HandLeft},
        {8, JointType.ShoulderRight},
        {9, JointType.ElbowRight},
        {10, JointType.WristRight},
        {11, JointType.HandRight},
        {12, JointType.HipLeft},
        {13, JointType.KneeLeft},
        {14, JointType.AnkleLeft},
        {15, JointType.FootLeft},
        {16, JointType.HipRight},
        {17, JointType.KneeRight},
        {18, JointType.AnkleRight},
        {19, JointType.FootRight},
        {20, JointType.SpineShoulder},
        {21, JointType.HandTipLeft},
        {22, JointType.ThumbLeft},
        {23, JointType.HandTipRight},
        {24, JointType.ThumbRight},
    };
}
