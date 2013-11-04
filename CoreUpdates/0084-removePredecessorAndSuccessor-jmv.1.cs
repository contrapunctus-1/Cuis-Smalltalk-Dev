'From Squeak3.7 of ''4 September 2004'' [latest update: #5989] on 6 November 2008 at 6:31:22 pm'!
	super setFont.
	stopConditions == DefaultStopConditions 
		ifTrue: [stopConditions := stopConditions copy].
	stopConditions at: Space asciiValue + 1 put: #space! !
	"While the section from start to stop has changed, composition may ripple all the way to the end of the text.  However in a rectangular container, if we ever find a line beginning with the same character as before (ie corresponding to delta in the old lines), then we can just copy the old lines from there to the end of the container, with adjusted indices and y-values"

	| newResult |
	newResult := OldTextComposer new 
				composeLinesFrom: start
				to: stop
				delta: delta
				into: lineColl
				priorLines: priorLines
				atY: startingY
				textStyle: textStyle
				text: text
				container: container.
	lines := newResult first asArray.
	maxRightX := newResult second.
	^maxRightX! !
	| newResult |
	self 
		OLDcomposeLinesFrom: 1
		to: text size
		delta: 0
		into: OrderedCollection new
		priorLines: Array new
		atY: container top.
	newResult := OldTextComposer new 
				composeLinesFrom: 1
				to: text size
				delta: 0
				into: OrderedCollection new
				priorLines: Array new
				atY: container top
				textStyle: textStyle
				text: text
				container: container.
	newResult first with: lines
		do: [:e1 :e2 | e1 longPrintString = e2 longPrintString ifFalse: [self halt]].
	newResult second = maxRightX ifFalse: [self halt].
	^{ 
		newResult.
		{ 
			lines.
			maxRightX}}! !
	| myLine lastChar |
	1 to: rectangles size
		do: 
			[:i | 
			currCharIndex <= theText size ifFalse: [^false].
			myLine := scanner 
						composeFrom: currCharIndex
						inRectangle: (rectangles at: i)
						firstLine: isFirstLine
						leftSide: i = 1
						rightSide: i = rectangles size.
			lines addLast: myLine.
			actualHeight := actualHeight max: myLine lineHeight.	"includes font changes"
			currCharIndex := myLine last + 1.
			lastChar := theText at: myLine last.
			lastChar = Character cr ifTrue: [^#cr]].
	^false! !
	"Having detected the end of rippling recoposition, we are only sliding old lines"

	| priorLine |
	prevIndex < prevLines size 
		ifFalse: 
			["There are no more prevLines to slide."

			^nowSliding := possibleSlide := false].

	"Adjust and re-use previously composed line"
	prevIndex := prevIndex + 1.
	priorLine := (prevLines at: prevIndex) slideIndexBy: deltaCharIndex
				andMoveTopTo: currentY.
	lines addLast: priorLine.
	currentY := priorLine bottom.
	currCharIndex := priorLine last + 1! !
	"Add text-related menu items to the menu"

	| outer |
	super addCustomMenuItems: aCustomMenu hand: aHandMorph.
	aCustomMenu 
		addUpdating: #autoFitString
		target: self
		action: #autoFitOnOff.
	aCustomMenu 
		addUpdating: #wrapString
		target: self
		action: #wrapOnOff.
	aCustomMenu add: 'text margins...' translated action: #changeMargins:.
	(Preferences noviceMode or: [Preferences simpleMenus]) 
		ifFalse: 
			[aCustomMenu add: 'code pane menu...' translated
				action: #yellowButtonActivity.
			aCustomMenu add: 'code pane shift menu....' translated
				action: #shiftedYellowButtonActivity].
	outer := self owner.
	((outer isKindOf: OldPolygonMorph) and: [outer isOpen]) 
		ifTrue: 
			[container isNil 
				ifFalse: 
					[aCustomMenu add: 'reverse direction' translated
						action: #reverseCurveDirection.
					aCustomMenu add: 'set baseline' translated action: #setCurveBaseline:]]
		ifFalse: 
			[(container isNil or: [container fillsOwner not]) 
				ifTrue: 
					[aCustomMenu add: 'fill owner''s shape' translated action: #fillingOnOff]
				ifFalse: 
					[aCustomMenu add: 'rectangular bounds' translated action: #fillingOnOff].
			(container isNil or: [container avoidsOcclusions not]) 
				ifTrue: 
					[aCustomMenu add: 'avoid occlusions' translated action: #occlusionsOnOff]
				ifFalse: 
					[aCustomMenu add: 'ignore occlusions' translated action: #occlusionsOnOff]]! !