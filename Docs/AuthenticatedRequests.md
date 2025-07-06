# Making Authenticated Requests to Admin APIs

## Overview

Admin APIs in the Training API require authentication with a valid Firebase token and admin privileges. 
This document shows how to properly send authenticated requests from a frontend application.

## Getting the Firebase Token

First, you need to get the ID token from the authenticated Firebase user:

