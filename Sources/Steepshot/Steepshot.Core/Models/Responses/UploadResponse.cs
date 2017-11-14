using System;

namespace Steepshot.Core.Models.Responses
{
    //    {
    //    "payload": {
    //        "title": "test",
    //        "tags": [
    //            "steepshot",
    //            "test",
    //            "steepshot"
    //        ],
    //        "username": "joseph.kalu",
    //        "body": "http://qa.steepshot.org/api/v1/image/02273668-788e-46cc-949b-cabde9bc2830.jpeg"
    //    },
    //    "meta": {
    //        "tags": [
    //            "steepshot",
    //            "test",
    //            "steepshot"
    //        ],
    //        "app": "steepshot/0.0.5",
    //        "extensions": [
    //            [
    //                0,
    //                {
    //                    "beneficiaries": [
    //                        {
    //                            "account": "steepshot",
    //                            "weight": 1000
    //                        }
    //                    ]
    //                }
    //            ]
    //        ]
    //    }
    //}
    public class UploadResponse
    {
        public ImageUploadResponse Payload { get; set; }

        public object Meta { get; set; }

        public Beneficiary[] Beneficiaries { get; set; }
    }
    
    public class Beneficiary
    {
        public string Account { get; set; }
        
        public UInt16 Weight { get; set; }
    }
}
