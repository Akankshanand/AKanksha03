' Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

Imports Microsoft.VisualStudio.ManagedInterfaces.ProjectDesigner

Namespace Microsoft.VisualStudio.Editors.PropertyPages

    ''' <summary>
    ''' A specialized property page site for hosting child pages from a PropPageUserControlBase page.
    '''   Supports undo and redo as a single transaction on the parent page's Undo stack.
    ''' </summary>
    Public Class ChildPageSite
        Implements IPropertyPageSiteInternal
        Implements IVsProjectDesignerPageSite

        ''' <summary>
        ''' The character that separates the property page type name from the property name in the special mangled
        '''   property names that we create.
        ''' </summary>
        Public Const NestingCharacter As String = ":"

        Private ReadOnly _wrappedInternalSite As IPropertyPageSiteInternal 'May *not* be Nothing
        Private ReadOnly _wrappedUndoSite As IVsProjectDesignerPageSite    'May be Nothing
        Private ReadOnly _nestedPropertyNamePrefix As String               'Prefix string to be placed at the beginning of PropertyName to distinguish properties from the page hosted by this child page site

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="childPage">The child page that is to be hosted (required).</param>
        ''' <param name="wrappedInternalSite">The IPropertyPageSiteInternal site (required).</param>
        ''' <param name="wrappedUndoSite">The IVsProjectDesignerPageSite site (optional).</param>
        Public Sub New(childPage As PropPageUserControlBase, wrappedInternalSite As IPropertyPageSiteInternal, wrappedUndoSite As IVsProjectDesignerPageSite)
            If childPage Is Nothing Then
                Debug.Fail("childPage missing")
                Throw New ArgumentNullException()
            End If
            If wrappedInternalSite Is Nothing Then
                Debug.Fail("Can't wrap a NULL site!")
                Throw New ArgumentNullException()
            End If
            _wrappedInternalSite = wrappedInternalSite
            _wrappedUndoSite = wrappedUndoSite
            _nestedPropertyNamePrefix = childPage.GetType.FullName & NestingCharacter
        End Sub

        ''' <summary>
        ''' Returns whether or not the property page hosted in this site should be with 
        '''   immediate-apply mode or not)
        ''' </summary>
        Private ReadOnly Property IsImmediateApply As Boolean Implements IPropertyPageSiteInternal.IsImmediateApply
            Get
                'Child pages are always non-immediate apply (we wait until the user clicks
                '  OK or Cancel)
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Delegate to the wrapped site
        ''' </summary>
        Public Function GetLocaleID() As Integer Implements IPropertyPageSiteInternal.GetLocaleID
            Return _wrappedInternalSite.GetLocaleID()
        End Function

        ''' <summary>
        ''' Delegate to the wrapped site
        ''' </summary>
        ''' <param name="ServiceType"></param>
        Public Function GetService(ServiceType As Type) As Object Implements IPropertyPageSiteInternal.GetService
            Return _wrappedInternalSite.GetService(ServiceType)
        End Function
        ''' <param name="flags"></param>
        Public Sub OnStatusChange(flags As PROPPAGESTATUS) Implements IPropertyPageSiteInternal.OnStatusChange
            ' We do *not* want to propagate this to our internal site - that would cause this change to
            ' be immediately applied, which is not what we want for child (modal) property pages...
        End Sub

        ''' <summary>
        ''' Instructs the page site to process a keystroke if it desires.
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <remarks>
        ''' This function can be called by a property page to give the site a chance to process a message
        '''   before the page does.  Return S_OK to indicate we have handled it, S_FALSE to indicate we did not
        '''   process it, and E_NOTIMPL to indicate that the site does not support keyboard processing.
        ''' </remarks>
        Public Function TranslateAccelerator(msg As Windows.Forms.Message) As Integer Implements IPropertyPageSiteInternal.TranslateAccelerator
            Return _wrappedInternalSite.TranslateAccelerator(msg)
        End Function

#Region "Undo/redo support for child pages"

        ''' <summary>
        ''' Get a localized name for the undo transaction.  This name appears in the
        '''   Undo/Redo history dropdown in Visual Studio.
        ''' Delegate to wrapped undo site.
        ''' </summary>
        ''' <param name="description"></param>
        Public Function GetTransaction(description As String) As ComponentModel.Design.DesignerTransaction Implements IVsProjectDesignerPageSite.GetTransaction
            If _wrappedUndoSite IsNot Nothing Then
                Return _wrappedUndoSite.GetTransaction(description)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Called by the child page when a change occurs on the page (during Apply).
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="propertyDescriptor"></param>
        ''' <param name="oldValue"></param>
        ''' <param name="newValue"></param>
        Public Sub OnPropertyChanged(propertyName As String, propertyDescriptor As ComponentModel.PropertyDescriptor, oldValue As Object, newValue As Object) Implements IVsProjectDesignerPageSite.OnPropertyChanged
            If _wrappedUndoSite IsNot Nothing Then
                _wrappedUndoSite.OnPropertyChanged(MungePropertyName(propertyName), propertyDescriptor, oldValue, newValue)
            End If
        End Sub

        ''' <summary>
        ''' Called by the child page when a change occurs on the page (during Apply).
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="propertyDescriptor"></param>
        Public Sub OnPropertyChanging(propertyName As String, propertyDescriptor As ComponentModel.PropertyDescriptor) Implements IVsProjectDesignerPageSite.OnPropertyChanging
            If _wrappedUndoSite IsNot Nothing Then
                _wrappedUndoSite.OnPropertyChanging(MungePropertyName(propertyName), propertyDescriptor)
            End If
        End Sub

        ''' <summary>
        ''' Munges a property name into a form that combines that type name of the child page that the
        '''   property came from.
        ''' </summary>
        ''' <param name="propertyName"></param>
        Private Function MungePropertyName(propertyName As String) As String
            'We need to mark properties as having coming from our hosted page.  We're forwarding undo/redo functionality to the same
            '  undo site (IVsPropertyDesignerPageSite) that handles the parent page, so that we create an undo/redo unit on the
            '  parent form that may be undone by the user.  But that means that the parent page will receive the requests (through
            '  IVsPropertyDesignerPage) for looking up and setting properties related to undo/redo functionality.  Prefixing the
            '  property name with the child page's type name lets the parent page know where to forward these requests.
            Return _nestedPropertyNamePrefix & propertyName
        End Function

#End Region

    End Class

End Namespace
