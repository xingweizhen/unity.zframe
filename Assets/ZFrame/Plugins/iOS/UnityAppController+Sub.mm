//
//  UnityAppController+Sub.m
//  Unity-iPhone
//
//  Created by xing weizhen on 2/8/15.
//
//

#import "UnityAppController+Sub.h"
#import "NSObject+SDKApi.h"
//#import "ReYunTrack.h"

@implementation UnityAppSubController : UnityAppController
- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [super applicationDidReceiveMemoryWarning:application];
    UnitySendMessage("AppController", "OnReceiveMemoryWarning", "");
}

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    //[ReYunChannel initWithappKey:@"776a21311eb82e645a95d6bc16e3a7a0" withChannelId:@"_default_"];
    
    if (![[NSUserDefaults standardUserDefaults] boolForKey:@"firstLaunch"]) {
        [[NSUserDefaults standardUserDefaults] setBool:YES forKey:@"firstLaunch"];
        // 首次启动
        
    } else {
        
    }
    
    return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

// UIAlertViewDelegate
- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    NSString *param = [[NSNumber numberWithInteger:buttonIndex] stringValue];
    UnitySendMessage("AppController", "OnAlertButtonClick", [param cStringUsingEncoding:NSUTF8StringEncoding]);
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(UnityAppSubController);
