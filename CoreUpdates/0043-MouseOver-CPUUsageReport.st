'From Squeak3.7 of ''4 September 2004'' [latest update: #5989] on 21 July 2008 at 11:05:10 am'!
	| promise process depth stack suspendedContext |
	promise := Processor tallyCPUUsageFor: 1 every: 10.
	
	[ | tally |
	tally := promise value.
		"OldWorldState addDeferredUIMessage: [self dumpTallyOnTranscript: tally]" 