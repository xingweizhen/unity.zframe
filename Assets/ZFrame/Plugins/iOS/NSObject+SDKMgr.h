//
//  NSObject+SDKMgr.h
//  Unity-iPhone
//
//  Created by xing weizhen on 11/10/14.
//
//

#import <Foundation/Foundation.h>

@interface SDKMgr : NSObject
+(void)SendSDKMessage:(NSDictionary *)dict;
+(NSDictionary *)JSONStringToDictionary:(NSString *)param;
@end
