# Micasa

I had been a long-term Picasa for Windows user when Google discontinued it.  Since 
Google declined to open source Picasa's source code, and there is no real alternative
available&mdash;for purchase or free&mdash;I eventually decided to code a partial clone.

## Background

My use of Picasa is to manage the family's library of photos: the family photo library 
is stored on a NAS on the home LAN and each user accesses the shared folders of photos.
While this works well as a way for us to share a library, it does present some 
challenges:
* when Picasa writes metadata to a photo there is a noticable delay;
* each user has their own local Picasa database, so when one user updates a photo's 
  metadata all other user's databases are out of date and don't automatically update 
  themselves;
	- this is especially problematic because metadata includes edits to photos that
	  haven't yet been 'saved' to the photo; and,
* the name associated with each face tag is different in each user's Picasa database.

These challenges became design constraints in my design of a partial Picasa clone.

## The Name

The design goal for this partial Picasa clone is to provide me&mdash;and anyone else who
might use the application&mdash;with a home for their photos.  This thought inspired the 
name Micasa as a corruption of the Spanish 'mi casa' (or, my house); which is also a
riff on Picasa.

## Key Features to Include

Micasa is intended to be a photo catalogue and touch-up programme modelled after Google’s 
Picasa application.  Key features to include:
* information about the pictures will be stored in .micasa files; modelled after Picasa’s 
  .picasa files (ASCII files)
    - timestamp .micasa updates (inside the file, just below each heading)
* two levels of .micasa files
	- with no file prefix the .micasa files applies to all photos in the folder
	- with a file prefix that exactly matches a photo’s filename, the file applies to that 
	  photo
	- if the prefix does not exactly match an existing photo’s filename, then it is ignored
* cache thumbnails, the catalogue index, and other information in a locally stored database
* allow the user to indicate that a folder/drive is remote; when a remote store is missing, 
  prompt the user for action to be taken (delete corresponding photos, ignore the missing 
  drives, etc., and remember that setting)
* do not process touch-up work; rather, store a log of the actions and apply it each time 
  the photo is displayed
	- however, provide the user with an option to apply the log
	- when the log is applied, a checkpoint image is stored to allow recovery back to the 
	  checkpoint by restoration of the saved image
    - when a log is first applied to a photo by Micasa, move the original file to a backup 
	  folder (modeled after Picasa) and put a copy in the original location—we’ll refer to 
	  these as checkpoint images, and they’ll have to be versioned
* allow the creation of Albums
* allow photos to be “starred”; but, allow “numbered stars” so that multiple lists can be 
  simultaneously kept—these are an Album sub-type
* at program start-up, or anytime it scans for new photos, changes to .micasa files must 
  also be detected — this is to allow photo stores to be shared
	- any change in a .micasa file will override data in the local database
	- edits to .micasa files should allow conflicting photo changes; that is, when Micasa 
	  detects a conflicting change, it should prompt and allow the user to create a “forked” 
	  copy of the picture or discard their own edit — it should never allow another user’s 
	  change to be over-written
* when photos in read-only stores are modified, including through placement in album, use 
  a special local .micasa file and local modified photo folder
	- the user should be able to specify the location of the “local”, writable folder where 
	  the photos and .micasa file(s) will be stored
	- this upholds the principle that the canonical store for all information is always a 
	  text file
	- the PhotoStore object should manage this read-only finesse
* photo stores must eventually include:
	- regular folders; absolute or relative references
	- URLs
	- mountable media (for example, CD-ROMs) that can be manually mounted on request by the 
	  user; that is, Micasa requests that a named volume be mounted by the user 

## Work to Date

My work on Micasa so far has been to create the framework for the actual processing and 
presentation of photos in the UI.  <em><strong>If you do decide to pull and compile Micasa, watch the
debug window for a trace of what Micasa is doing as it scans the file system.</strong></em>
