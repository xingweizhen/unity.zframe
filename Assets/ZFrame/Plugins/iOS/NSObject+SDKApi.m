//
//  NSObject+SDKApi.m
//  Unity-iPhone
//
//  Created by xing weizhen on 11/10/14.
//
//

#import "NSObject+SDKApi.h"
#import "NSObject+SDKMgr.h"
//#import "ReYunTrack.h"
//#import "IAppPayOpenIdSDK.h"
//#import "IAPManager.h"

@implementation SDKApi : NSObject
-(NSString *)ProcessingData:(NSString *)param
{
    NSLog(@"ProcessingData: %@", param);
    NSDictionary* dict = [SDKMgr JSONStringToDictionary:param];
    NSString* method = [dict valueForKey:@"method"];
    if ([method isEqualToString:@"OnRegisted"]) {
        //[ReYunChannel setRegisterWithAccountID:[dict valueForKey:@"account"]];
    } else if ([method isEqualToString:@"OnLogined"]) {
        //[ReYunChannel setLoginWithAccountID:[dict valueForKey:@"account"]];
    } else if ([method isEqualToString:@"OnStartPay"]) {
        NSString* orderNumber = [dict valueForKey:@"order"];
        NSString* paymentType = [dict valueForKey:@"payment"];
        NSString* currencyType = [dict valueForKey:@"currency"];
        NSNumber* currencyAmount = [dict valueForKey:@"amount"];
//        [ReYunChannel setPaymentStart:orderNumber
//                          paymentType:paymentType
//                          currentType:currencyType
//                       currencyAmount:[currencyAmount floatValue]];
    } else if ([method isEqualToString:@"OnPay"]) {
        NSString* orderNumber = [dict valueForKey:@"order"];
        NSString* paymentType = [dict valueForKey:@"payment"];
        NSString* currencyType = [dict valueForKey:@"currency"];
        NSNumber* currencyAmount = [dict valueForKey:@"amount"];
//        [ReYunChannel setPayment:orderNumber
//                     paymentType:paymentType
//                     currentType:currencyType
//                  currencyAmount:[currencyAmount floatValue]];
    } else if ([method isEqualToString:@"OnEvent"]) {
        //[ReYunChannel setEvent:[dict valueForKey:@"event"]];
    } else if ([method isEqualToString:@"getPid"]) {
        return @"201";
    } else if ([method isEqualToString:@"doLogout"]) {
//        [[IAppPayOpenIdSDK sharedInstance] loginOutWithresultBlock:^(BOOL isLogOut)
//        {
//            if (isLogOut)
//            {
//                [[IAppPayOpenIdSDK sharedInstance] showWindowWithAppId:@"3011372648" isForceLogin:YES];
//            }
//        }];
    } else if ([method isEqualToString:@"iapPurchase"])
    {
        // 苹果支付
        NSString* id = [dict valueForKey:@"param"];
        //[[IAPManager Instance] buyRequest:id];
        return @"iap";
    }
    return NULL;
}
-(void)OnAppLaunch
{
    // 初始化苹果支付
//    [[IAPManager Instance] attachObserver];
//
//    [[IAppPayOpenIdSDK sharedInstance] showWindowWithAppId:@"3011372648" isForceLogin:YES];
    
    // 设置登录回调block，此block在只有在离开openId界面时执行
//    [[IAppPayOpenIdSDK sharedInstance] setResultInfoBlock:^(NSDictionary *resultInfo) {
//        NSString *result = [NSString stringWithFormat:@"result:%@ ", [resultInfo description]];
//
//        NSLog(@"%@", result);
//
//        if ([resultInfo objectForKey:@"resultType"] == @"loginInfo"){
//            NSDictionary *dic;
//            dic = [NSDictionary dictionaryWithObjectsAndKeys:
//                    @"sdk_logined",@"method",
//                    [resultInfo objectForKey:@"userID"],@"uid",
//                    [resultInfo objectForKey:@"loginToken"],@"token",
//                    nil];
//
//            [SDKMgr SendSDKMessage:dic];
//        }
//    }];
}
static SDKApi *inst;
+(SDKApi *)Instance
{
    if (inst == NULL) {
        inst = [[SDKApi alloc]init];
    }
    return inst;
}
@end
