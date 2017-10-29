#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

static int quickAction = 0;
/// 1 = new single player game
/// 2 = new multi player game
int SciFiGetQuickAction() {
    return quickAction;
}

@interface SFQuickActions : NSObject
@end

@implementation SFQuickActions

+ (void)load {
    [[NSNotificationCenter defaultCenter]
        addObserver: self
           selector: @selector(checkQuickActions:)
               name: @"UIApplicationDidFinishLaunchingNotification"
             object: nil];
}

+ (void)checkQuickActions:(NSNotification *)notification {
    NSDictionary *launchOptions = [notification userInfo];
    UIApplicationShortcutItem *item = [launchOptions objectForKey: @"UIApplicationLaunchOptionsShortcutItemKey"];
    if (item == nil) {
        return;
    }
    if ([item.type isEqualToString:@"Singleplayer"]) {
        quickAction = 1;
    } else if ([item.type isEqualToString:@"Multiplayer"]) {
        quickAction = 2;
    }
}

@end
