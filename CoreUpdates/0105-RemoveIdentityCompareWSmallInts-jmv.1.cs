'From Squeak3.7 of ''4 September 2004'' [latest update: #5989] on 9 December 2008 at 5:09:49 pm'!
	"Modifies entries on aMatrix to make it the Householder transforms
	that puts zeroes at column i of the receiver. If forQR is false, the
	product of aMatrix * self is Hessemberg superior, otherways its a
	triangular matrix. "

	| x xNorm2Squared v vNorm2Squared element i |
	i _ j - (forQR ifTrue: [ 1 ] ifFalse: [ 0 ]).
	x _ self appropriateResultClass newVectorSize: self height-i.
	xNorm2Squared _ 0.
	1 to: x height do: [ :ii |
		element _ self i: ii+i j: j.
		xNorm2Squared _ xNorm2Squared + element squared.
		x i: ii j: 1 put: element ].
	v _ x.
	"If column already has zeros, do nothing"
	xNorm2Squared = 0.0 ifTrue: [ ^false ].
	"If column already has zeros, do nothing. If forQR = false, then the first element in x
	could not be zero, and anyway there's nothing to do"
	(forQR not and: [ xNorm2Squared = (x i: 1 j: 1) squared ]) ifTrue: [ ^false ].

	v i: 1 j: 1 put: (v i: 1 j: 1) + xNorm2Squared sqrt.
	vNorm2Squared _ v norm2Squared.

	1 to: i do: [ :ii |
		aMatrix i: ii j: ii put: 1.
		ii+1 to: aMatrix width do: [ :jj |
			aMatrix i: ii j: jj put: 0.
			aMatrix i: jj j: ii put: 0 ] ].
	1 to: x height do: [ :ii |
		1 to: x height do: [ :jj |
			aMatrix i: ii+i j: jj+i put: 
				(ii=jj ifTrue:[1] ifFalse:[0]) - 
				(2.0 / vNorm2Squared * (v i: ii j: 1) * (v i: jj j: 1) ) ] ].
	^true! !
	"Add standard halo items to the menu"

	| unlockables |

	self isWorldMorph ifTrue:
		[^ self addWorldHaloMenuItemsTo: aMenu hand: aHandMorph].

	aMenu add: 'send to back' translated action: #goBehind.

	self addFillStyleMenuItems: aMenu hand: aHandMorph.
	self addBorderStyleMenuItems: aMenu hand: aHandMorph.
	self addLayoutMenuItems: aMenu hand: aHandMorph.
	self addHaloActionsTo: aMenu.
	owner isTextMorph ifTrue:[self addTextAnchorMenuItems: aMenu hand: aHandMorph].
	aMenu addLine.
	self addToggleItemsToHaloMenu: aMenu.
	aMenu addLine.
	self addCopyItemsTo: aMenu.
	self addExportMenuItems: aMenu hand: aHandMorph.
	Preferences noviceMode ifFalse:
		[self addDebuggingItemsTo: aMenu hand: aHandMorph].

	aMenu addLine.
	aMenu defaultTarget: self.

	aMenu addLine.

	unlockables _ self submorphs select:
		[:m | m isLocked].
	unlockables size = 1 ifTrue:
		[aMenu
			add: ('unlock "{1}"' translated format: unlockables first externalName)
			action: #unlockContents].
	unlockables size > 1 ifTrue:
		[aMenu add: 'unlock all contents' translated action: #unlockContents.
		aMenu add: 'unlock...' translated action: #unlockOneSubpart].

	aMenu defaultTarget: aHandMorph.
! !
	"Add standard halo items to the menu, given that the receiver is a World"

	| unlockables |
	self addFillStyleMenuItems: aMenu hand: aHandMorph.
	self addLayoutMenuItems: aMenu hand: aHandMorph.

	aMenu addLine.
	self addWorldToggleItemsToHaloMenu: aMenu.
	aMenu addLine.
	self addCopyItemsTo: aMenu.
	self addExportMenuItems: aMenu hand: aHandMorph.

	Preferences noviceMode ifFalse:
		[self addDebuggingItemsTo: aMenu hand: aHandMorph].

	aMenu addLine.
	aMenu defaultTarget: self.

	aMenu addLine.

	unlockables _ self submorphs select:
		[:m | m isLocked].
	unlockables size = 1 ifTrue:
		[aMenu add: ('unlock "{1}"' translated format:{unlockables first externalName})action: #unlockContents].
	unlockables size > 1 ifTrue:
		[aMenu add: 'unlock all contents' translated action: #unlockContents.
		aMenu add: 'unlock...' translated action: #unlockOneSubpart].

	aMenu defaultTarget: aHandMorph.
! !
	"ugh"

	| plug |
	^paneMorphs size = 1 and: 
			[((plug := paneMorphs first) isKindOf: OldPluggableTextMorph) 
				and: [plug model isKindOf: TranscriptStream]]! !
	"If the receiver represents a workspace, return an Association between the title and that text, else return nil"

	(paneMorphs size ~= 1 
		or: [(paneMorphs first isKindOf: OldPluggableTextMorph) not]) 
			ifTrue: [^nil].
	^labelString -> paneMorphs first text! !
	"Create and schedule an Inspector on an element of the receiver's model's currently selected collection."

	| sel selSize countString count nameStrs |
	self selectionIndex = 0 ifTrue: [^self changed: #flash].
	((sel := self selection) isKindOf: SequenceableCollection) 
		ifFalse: 
			[(sel isKindOf: OldMorphExtension) ifTrue: [^sel inspectElement].
			^sel inspect].
	(selSize := sel size) = 1 ifTrue: [^sel first inspect].
	selSize <= 20 
		ifTrue: 
			[nameStrs := (1 to: selSize) asArray collect: 
							[:ii | 
							ii printString , '   ' 
								, (((sel at: ii) printStringLimitedTo: 25) replaceAll: Character cr
										with: Character space)].
			count := PopUpMenu withCaption: 'which element?' chooseFrom: nameStrs.
			count = 0 ifTrue: [^self].
			^(sel at: count) inspect].
	countString := FillInTheBlank 
				request: 'Which element? (1 to ' , selSize printString , ')'
				initialAnswer: '1'.
	countString isEmptyOrNil ifTrue: [^self].
	count := Integer readFrom: (ReadStream on: countString).
	(count > 0 and: [count <= selSize]) 
		ifTrue: [(sel at: count) inspect]
		ifFalse: [Beeper beep]! !
	"Save the receiver's contents string to a file, prompting the user for a file-name.  Suggest a reasonable file-name."

	| fileName stringToSave parentWindow labelToUse suggestedName lastIndex |
	stringToSave := paragraph text string.
	stringToSave size = 0 ifTrue: [^self inform: 'nothing to save.'].
	parentWindow := self model dependents 
				detect: [:dep | dep isKindOf: OldSystemWindow]
				ifNone: [nil].
	labelToUse := parentWindow ifNil: ['Untitled']
				ifNotNil: [parentWindow label].
	suggestedName := nil.
	#(#('Decompressed contents of: ' '.gz')) do: 
			[:leaderTrailer | 
			"can add more here..."

			(labelToUse beginsWith: leaderTrailer first) 
				ifTrue: 
					[suggestedName := labelToUse copyFrom: leaderTrailer first size + 1
								to: labelToUse size.
					(labelToUse endsWith: leaderTrailer last) 
						ifTrue: 
							[suggestedName := suggestedName copyFrom: 1
										to: suggestedName size - leaderTrailer last size]
						ifFalse: 
							[lastIndex := suggestedName lastIndexOf: $. ifAbsent: [0].
							(lastIndex = 0 or: [lastIndex = 1]) 
								ifFalse: [suggestedName := suggestedName copyFrom: 1 to: lastIndex - 1]]]].
	suggestedName ifNil: [suggestedName := labelToUse , '.text'].
	fileName := FillInTheBlank request: 'File name?'
				initialAnswer: suggestedName.
	fileName isEmptyOrNil 
		ifFalse: 
			[(FileStream newFileNamed: fileName)
				nextPutAll: stringToSave;
				close]! !
	"Offer up the common-requests menu.  If the user chooses one, then evaluate it, and -- provided the value is a number or string -- show it in the Transcript."

	"Utilities offerCommonRequests"

	| aMenu strings |
	(CommonRequestStrings == nil or: [CommonRequestStrings isKindOf: Array]) 
		ifTrue: [self initializeCommonRequestStrings].
	strings := CommonRequestStrings contents.
	aMenu := OldMenuMorph new.
	aMenu title: 'Common Requests' translated.
	aMenu addStayUpItem.
	strings asString linesDo: 
			[:aString | 
			aString = '-' 
				ifTrue: [aMenu addLine]
				ifFalse: 
					[aString size = 0 ifTrue: [aString := ' '].
					aMenu 
						add: aString
						target: self
						selector: #eval:
						argument: aString]].
	aMenu addLine.
	aMenu 
		add: 'edit this list' translated
		target: self
		action: #editCommonRequestStrings.
	aMenu popUpInWorld: self currentWorld! !