-- 
-- Abstract: Hello World sample app.
--
-- Version: 1.2
-- 
-- Sample code is MIT licensed
-- Copyright (C) 2014 Corona Labs Inc. All Rights Reserved.
--
-- Supports Graphics 2.0
------------------------------------------------------------

local background = display.newImage( "world.jpg", display.contentCenterX, display.contentCenterY )

function getLoginInfo()
	local requestingLoginEvent =
	{
		name = "requestingLogin",
		username = "",
	}
	print("displatching login event")
	Runtime:dispatchEvent(requestingLoginEvent)
end


-- Called when the C# login popup has been closed.
function retrieveLoginInfo(event)
	-- Fetch the login name and password. Do whatever you got to do here.
	if event.submitted then
		local username = event.username
		local password = event.password
		print("Retrieved login info", username, password)
	else
		print("Login was canceled.")
	end
end
Runtime:addEventListener("onLoginInfo", retrieveLoginInfo)

local myText = display.newText( "Tap to login!", display.contentCenterX, display.contentHeight / 3, native.systemFont, 40 )
myText:setFillColor( 1.0, 0.4, 0.4 )
myText:addEventListener( "tap", getLoginInfo )

local myUsername = display.newText( "", display.contentCenterX, display.contentHeight / 3 * 2, native.systemFont, 40)
myUsername:setFillColor( 1, 1, 1 )
