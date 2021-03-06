﻿Imports System
Imports System.IO
Imports System.Xml
Imports System.Xml.XPath
Imports System.Xml.Xsl
Partial Class svg_component_custom_svgmachine
	Inherits System.Web.UI.Page
	
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		' Set URL base paths
		Dim strApplicationRoot As String = "http://www.sleepingdog.org.uk/svg/component/custom/"
		Dim strTransformationRoot As String = "http://www.sleepingdog.org.uk/svg/component/Transformations/"
		' Get input from query string parameters, of the form:
		' ?graphic=scene1.svg&set=rollergirls.svg&alcohol=false
		' Take the graphic path from the query string parameter.
		Dim strSvgGraphicPath As String = strApplicationRoot + Request.QueryString("graphic") ' such as "scene.svg" 
		' Take the alternate library set from the query string parameter.
		Dim strReplacementSetUrl As String = strApplicationRoot + Request.QueryString("set") ' such as "cricketers.svg" or "rollergirls.svg"
		' Get personalisation styles from query string parameters
		' &flesh=#E2CCCC&hair=#778899&hat=#FFF5EE&clothes=#F5DEB3&head=1
		Dim strFlesh As String = Request.QueryString("flesh") ' such as #E2CCCC
		Dim strHair As String = Request.QueryString("hair") ' such as #778899
		Dim strHat As String = Request.QueryString("hat") ' such as #FFF5EE
		Dim strClothes As String = Request.QueryString("clothes") ' such as #F5DEB3
		Dim intHead As Integer = CInt(Request.QueryString("head"))
		
		' Create personalisation style sheet
		Dim strPersonMeStyles As String = ".personmeflesh{fill:" & strFlesh & ";stroke:#B49999;stroke-miterlimit:10;}" & vbCrlf & _
		".personmehair{fill:" & strHair & ";stroke:#6A4058;}" & vbCrlf & _
		".personmehat{fill:" & strHat & ";stroke:#66666B;stroke-miterlimit:10;}" & vbCrlf & _
		".personmeclothes{fill:" & strClothes & ";stroke:#CCC0C0;stroke-miterlimit:10;}"
		For head = 1 to 4
			If head = intHead Then 'make selected head style visible
				strPersonMeStyles &= vbCrlf & "/* head style " & CStr(intHead) & " */" & vbCrlf
			Else 'hide non-selected head styles
				strPersonMeStyles &= vbCrlf & "#person-me-front-head-option-" & CStr(head) & ", #person-me-back-head-option-" & CStr(head) & ", #person-me-right-head-option-" & CStr(head) & _
					" {display: none;}" & vbCrlf
			End If
		Next head
 		
		Dim booAlcohol As Boolean
		If Request.QueryString("alcohol") = "true" Or Request.QueryString("alcohol") Is Nothing Then
			booAlcohol = True
			Else booAlcohol = False
		End If
		Dim booMeat As Boolean
		If Request.QueryString("meat") = "true" Or Request.QueryString("meat") Is Nothing Then
			booMeat = True
			Else booMeat = False
		End If
		
		' Define xpath document to be carried into further transformations
		Dim xpdDoc1 As XPathDocument
		If Request.QueryString("set") Is Nothing Then
			xpdDoc1 = New XPathDocument(strSvgGraphicPath)
		Else
			' The first transformation (optionally) replaces components like person1 and person2 from a selected alternate graphic set
			' Compile the first style sheet.
			Dim xctTransform1 As New XslCompiledTransform()
			' Choose the transform stylesheet
			Dim strStylesheetUri1 As String = strTransformationRoot + "svg-person-replacement.xslt"
			' Choose the source XML document.
			Dim strDocumentUri1 As String = strSvgGraphicPath
			' Create the XsltSettings object with document() enabled.
			Dim settings1 As New XsltSettings(True, False)
			' Create the XslCompiledTransform object and load the style sheet.
			xctTransform1.Load(strStylesheetUri1, settings1, New XmlUrlResolver())
			' Create the XsltArgumentList.
			Dim argList1 As XsltArgumentList = New XsltArgumentList()
			' Add parameters
			argList1.AddParam("replacementSetUrl", "", strReplacementSetUrl)
			' Create a memory stream to carry the output of the transformation.
			Dim ms1 As MemoryStream = New MemoryStream()
			' Transform the RDF to QTI Lite
			xctTransform1.Transform(strDocumentUri1, argList1, ms1)
			' Load the results into an XPathDocument object.
			ms1.Seek(0, SeekOrigin.Begin)
			xpdDoc1 = New XPathDocument(ms1)
		End If
		
		' The second transformation (optionally) removes or otherwise treats classes of labelled components like alcohol or meat
		' Create a memory stream to carry the output of the transformation
		Dim ms2 As MemoryStream = New MemoryStream()
		Dim xctTransform2 As XslCompiledTransform = New XslCompiledTransform()
		Dim strStylesheetUri2 As String = strTransformationRoot + "svg-object-class-treatment.xslt"
		Dim settings2 As New XsltSettings(True, False)
		xctTransform2.Load(strStylesheetUri2, settings2, New XmlUrlResolver())
		Dim argList2 As XsltArgumentList = New XsltArgumentList()
		argList2.AddParam("alcohol", "", booAlcohol)
		argList2.AddParam("meat", "", booMeat)
		argList2.AddParam("personstyles", "", strPersonMeStyles)
		xctTransform2.Transform(xpdDoc1, argList2, ms2)
		ms2.Seek(0, SeekOrigin.Begin)
		Dim xpdDoc2 As XPathDocument = New XPathDocument(ms2)

		Dim xpnDoc2 As XPathNavigator
		xpnDoc2 = xpdDoc2.CreateNavigator
		xmlSVG.XPathNavigator = xpnDoc2
	End Sub
End Class
