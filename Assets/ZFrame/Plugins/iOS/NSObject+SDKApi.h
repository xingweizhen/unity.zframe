//
//  NSObject+SDKApi.h
//  Unity-iPhone
//
//  Created by xing weizhen on 11/10/14.
//
//

#import <Foundation/Foundation.h>

@interface SDKApi : NSObject
-(NSString *)ProcessingData:(NSString *)param;
-(void)OnAppLaunch;
+(SDKApi *)Instance;
@end
