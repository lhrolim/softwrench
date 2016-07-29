//
//  CDVCookieMaster.m
//  
//
//  Created by Kristian Hristov on 12/16/14.
//
//

#import "CDVCookieMaster.h"


@implementation CDVCookieMaster

 - (void)getCookieValue:(CDVInvokedUrlCommand*)command
{
    CDVPluginResult* pluginResult = nil;
    NSString* urlString = [command.arguments objectAtIndex:0];
    __block NSString* cookieName = [command.arguments objectAtIndex:1];
    
    if (urlString != nil) {
        NSArray* cookies = [[NSHTTPCookieStorage sharedHTTPCookieStorage] cookiesForURL:[NSURL URLWithString:urlString]];
        
        __block NSString *cookieValue;
        
        [cookies enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
            NSHTTPCookie *cookie = obj;
            if([cookie.name isEqualToString:cookieName])
            {
                cookieValue = cookie.value;
                *stop = YES;
            }
        }];
        if (cookieValue != nil) {
            pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_OK messageAsDictionary:@{@"cookieValue":cookieValue}];
        } else {
            pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_ERROR messageAsString:@"No cookie found"];
        }

    } else {
        pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_ERROR messageAsString:@"URL was null"];
    }
    [self.commandDelegate sendPluginResult:pluginResult callbackId:command.callbackId];
}

 - (void)setCookieValue:(CDVInvokedUrlCommand*)command
{
    //[[NSHTTPCookieStorage sharedHTTPCookieStorage] setCookieAcceptPolicy:NSHTTPCookieAcceptPolicyAlways];
    
    CDVPluginResult* pluginResult = nil;
    NSString* urlString = [command.arguments objectAtIndex:0];
    NSString* cookieName = [command.arguments objectAtIndex:1];
    NSString* cookieValue = [command.arguments objectAtIndex:2];

     NSMutableDictionary *cookieProperties = [NSMutableDictionary dictionary];
     [cookieProperties setObject:cookieName forKey:NSHTTPCookieName];
     [cookieProperties setObject:cookieValue forKey:NSHTTPCookieValue];
     [cookieProperties setObject:urlString forKey:NSHTTPCookieOriginURL];
     [cookieProperties setObject:@"/" forKey:NSHTTPCookiePath];

    NSHTTPCookie *cookie = [NSHTTPCookie cookieWithProperties:cookieProperties];
    [[NSHTTPCookieStorage sharedHTTPCookieStorage] setCookie:cookie];
    
    NSArray* cookies = [NSArray arrayWithObjects:cookie, nil];
    
    NSURL *url = [[NSURL alloc] initWithString:urlString];
    [[NSHTTPCookieStorage sharedHTTPCookieStorage] setCookies:cookies forURL:url mainDocumentURL:nil];

    pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_OK messageAsString:@"Set cookie executed"];
    [self.commandDelegate sendPluginResult:pluginResult callbackId:command.callbackId];
}

-(void)setCookieOption:(CDVInvokedUrlCommand*)command
{
    CDVPluginResult* pluginResult = nil;
    NSMutableDictionary* options = [command.arguments objectAtIndex:0];
    NSDictionary *properties = [self mapCookieProperties:options];
    NSLog(@"Setting cookie with properties: %@", properties);
    
    NSHTTPCookie *cookie = [NSHTTPCookie cookieWithProperties:properties];
    [[NSHTTPCookieStorage sharedHTTPCookieStorage] setCookie:cookie];
    
    pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_OK messageAsString:@"Set cookie executed"];
    [self.commandDelegate sendPluginResult:pluginResult callbackId:command.callbackId];}

- (void)clearCookies:(CDVInvokedUrlCommand*)command
{
    NSHTTPCookie *cookie;
    NSHTTPCookieStorage *storage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
    for (cookie in [storage cookies]) {
        [storage deleteCookie:cookie];
    }
    [[NSUserDefaults standardUserDefaults] synchronize];
    
    CDVPluginResult* pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_OK];
    [self.commandDelegate sendPluginResult:pluginResult callbackId:command.callbackId];
}


-(NSDictionary*)mapCookieProperties:(NSDictionary*)options {
    NSDictionary *cookiePropertiesMap = @{
      @"comment": NSHTTPCookieComment,
      @"commentUrl": NSHTTPCookieCommentURL,
      @"discard": NSHTTPCookieDiscard,
      @"domain": NSHTTPCookieDomain,
      @"expires": NSHTTPCookieExpires,
      @"maximumAge": NSHTTPCookieMaximumAge,
      @"name": NSHTTPCookieName,
      @"originUrl": NSHTTPCookieOriginURL,
      @"path": NSHTTPCookiePath,
      @"port": NSHTTPCookiePort,
      @"secure": NSHTTPCookieSecure,
      @"value": NSHTTPCookieValue,
      @"version": NSHTTPCookieVersion,
    };
    NSMutableDictionary* properties = [NSMutableDictionary dictionary];
    for (NSString *key in options) {
        id value = [options objectForKey:key];
        if ([key isEqual: @"expires"]) {
            NSTimeInterval time = (int) value;
            value = [[NSDate date] dateByAddingTimeInterval:time];
        }
        NSString *property = [cookiePropertiesMap objectForKey:key];
        if (value && property) {
            [properties setObject:value forKey:property];
        }
    }
    return properties;
}

@end
