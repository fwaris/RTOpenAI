namespace WebFlows

module FlowDefs =
    let vzFlow rootUrl = 
        let steps  =
                [
                    Page rootUrl
                    Clicks [
                        { ElemRef.Default with elementId = Some "gnav20-sign-id-mobile"}
                    ]
                    Clicks [
                        { ElemRef.Default with elementId = Some "gnav20-sign-id-list-item-1-mobile"}
                    ]
                    Pause {|label="Login"; altFlowId="login"|}
                    Clicks [
                        { ElemRef.Default with aria_label = Some "Account menu list"}
                    ]
                    Clicks [
                        { ElemRef.Default with elementId = Some "gnav20-Account-L2-2-mobile"}                    
                    ]
                    Clicks [
                        { ElemRef.Default with elementId = Some "gnav20-Account-L3-29-mobile"}                    
                    ]
                    Clicks [
                        { ElemRef.Default with aria_label = Some "Contact and Billing"}                    
                    ]                    
                    Clicks [
                        { ElemRef.Default with path = Some """ion-item[data-track*="manage_addresses"]"""}                    
                    ]
                    Clicks [
                        { ElemRef.Default with path = Some """ion-item[data-track*="billing_address"]"""}                    
                    ]
                    Clicks [
                        { ElemRef.Default with path = Some """button[data-testid="Cancel"]"""}                    
                    ]
                    Clicks [
                        { ElemRef.Default with aria_label = Some "Security"}                    
                    ]
                    Clicks [
                        { ElemRef.Default with path = Some """ion-item[data-track*="transfer_pin"]"""}                    
                    ]
                    Await [
                        { ElemRef.Default with aria_label = Some "Generate PIN"}                    
                    ]
                    //Pause {|label="Auth for pin"; altFlowId="mfa"|} //not performed repeatedly due to risk of lockout 
                    Done "done"
                ]
        let extractions =
            [
                { Name="account number"; ElemRef ={ElemRef.Default with xpath=Some """//p[contains(., 'Account number:')]/span[@data-cs-mask='true']"""}}
                { Name="zip"; ElemRef ={ElemRef.Default with path=Some """input[data-testid="zipCode"]"""}}
                { Name="pin"; ElemRef ={ ElemRef.Default with xpath = Some """//h2[contains(., "Here's your Number Transfer PIN.")]/following::h2[1]"""}}
                { Name="pin page account"; ElemRef ={ ElemRef.Default with xpath = Some """//h6[contains(.,'For Account Number')]/following::p[1]"""}}
            ]
                
        {
            FlowId="vz"
            Path = steps
            Extractions = extractions
        }

