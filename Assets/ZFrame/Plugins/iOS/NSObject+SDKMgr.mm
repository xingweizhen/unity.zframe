//
//  NSObject+SDKMgr.m
//  Unity-iPhone
//
//  Created by xing weizhen on 11/10/14.
//
//

#import "NSObject+SDKApi.h"
#import "NSObject+SDKMgr.h"

@implementation SDKMgr : NSObject
+(void)SendSDKMessage:(NSDictionary *)dict
{
    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:0 error:&error];
    if (error == nil) {
        NSString *param = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        UnitySendMessage("SDKMgr", "OnSDKMessage", [param cStringUsingEncoding:NSUTF8StringEncoding]);
    } else {
        NSLog(@"%@", @"错误的JSON数据结构");
    }
}
+(NSDictionary *)JSONStringToDictionary:(NSString *)param
{
    NSData *json = [param dataUsingEncoding:NSUTF8StringEncoding];
    NSError *error = nil;
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:json options:NSJSONReadingAllowFragments error:&error];
    if (error == nil) {
        return dict;
    } else {
        NSLog(@"%@", error);
        return nil;
    }
}

@end
extern "C" {
    NSString* CreateNSString(const char * string)
    {
        if (string != NULL) {
            return ([NSString stringWithUTF8String: string]);
        } else {
            return ([NSString stringWithUTF8String: ""]);
        }
    }
    
    char *ProcessingData(char *param)
    {
        NSString* ret = [[SDKApi Instance]ProcessingData:CreateNSString(param)];
        return ret != NULL ? strdup([ret cStringUsingEncoding:NSUTF8StringEncoding]) : (char *)nullptr;
    }
    
    void OnAppLaunch(void)
    {
        if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 8.0) {
            UIUserNotificationType myTypes = UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeAlert | UIRemoteNotificationTypeSound;
            UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:myTypes categories:nil];
            [[UIApplication sharedApplication] registerUserNotificationSettings:settings];
        } else {
            UIRemoteNotificationType myTypes = UIRemoteNotificationTypeBadge|UIRemoteNotificationTypeAlert|UIRemoteNotificationTypeSound;
            [[UIApplication sharedApplication] registerForRemoteNotificationTypes:myTypes];
        }
        [UIApplication sharedApplication].applicationIconBadgeNumber = 0;
        [[SDKApi Instance]OnAppLaunch];
    }
    
    void setApplicationIconBadgeNumber(int n)
    {
        [[UIApplication sharedApplication] setApplicationIconBadgeNumber:n];
    }
    
    long getDirectoryStorage(char *path)
    {
        NSString *dir = CreateNSString(path);
        NSDictionary *fattributes = [[NSFileManager defaultManager] attributesOfFileSystemForPath:dir error:NULL];
        NSNumber *nbytes = [fattributes objectForKey:NSFileSystemFreeSize];
        return[nbytes longLongValue];
    }
    
    void Alert(char *jsonStr)
    {
        NSData *json = [CreateNSString(jsonStr) dataUsingEncoding:NSUTF8StringEncoding];
        NSError *error = nil;
        NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:json options:NSJSONReadingAllowFragments error:&error];
        if (error == nil) {
            UIAlertView *alert = [[UIAlertView alloc]
                                  initWithTitle:[dict objectForKeyedSubscript:@"title"]
                                  message:[dict objectForKey:@"message"]
                                  delegate:[UIApplication sharedApplication].delegate
                                  cancelButtonTitle:[dict objectForKey:@"cancel"]
                                  otherButtonTitles:nil];
            NSString *title = [dict objectForKey:@"confirm"];
            if ([title compare:@""] != NSOrderedSame) {
                [alert addButtonWithTitle:title];
            }
            [alert show];
        }
    }
}
