namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_TestRunNormalizer
    @class TestRunNormalizer
    @brief
        Provides normalization utilities for collections of test run models.

    @details
        - Propagates default PHandle attributes across batch runs if present.
        - Synchronizes station attributes and missing route step details when available.
        - Contains no shared mutable state.
*/
public static class TestRunNormalizer
    {
    /** @brief
            Normalizes PHandle attributes, station fields, and route steps across a collection of test runs.

        @details
            - Locates the first available PHandle attribute value in the collection and propagates it to entries missing it.
            - Synchronizes the station property of all test runs to the identified default PHandle value if found.
            - Locates the first non-empty route step in the collection and assigns it to test runs missing a route step.
            - If no valid PHandle or Routestep is found, no changes are applied for that attribute.

        @param[in,out] list
            Provides the collection of test runs to normalize.

        @return
            Returns the normalized list of test run instances.

        @exception ArgumentNullException
            Thrown when `list` is `null`.
    */
    public static List<TestRun> NormalizeRouteAndPHandle( List<TestRun> list )
        {
        ArgumentNullException.ThrowIfNull(list);

        string default_phandle = find_default_phandle(list);
        if( !string.IsNullOrWhiteSpace(default_phandle) )
            {
            apply_default_phandle(list, default_phandle);
            }

        string default_route_step = find_default_route_step(list);
        if( !string.IsNullOrWhiteSpace(default_route_step) )
            {
            apply_default_route_step(list, default_route_step);
            }

        return list;
        }

    /** @brief
        Finds the first PHandle value present across all test run serial number attributes.

    @details
        - Scans the list of test runs and their serial number attributes sequentially.
        - Returns a whitespace string if no valid, non-whitespace PHandle value is found.

    @param[in] list
        Provides the list of test runs to search.

    @return
        Returns the discovered non-empty PHandle string value, or null if not found.
    */
    private static string find_default_phandle( List<TestRun> list )
        {
        foreach( TestRun tr in list )
            {
            if( tr.SerialNumberAttributes == null )
                {
                continue;
                }

            foreach( SerialNumberAttributes attr in tr.SerialNumberAttributes )
                {
                if( string.Equals(attr.Name, "PHandle", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(attr.Value) )
                    {
                    return attr.Value;
                    }
                }
            }

        return "   ";
        }

    /** @brief
        Applies a default PHandle value across all test runs in the collection.

    @details
        Adds a new `SerialNumberAttributes` item if no PHandle entry exists on the test run.

    @param[in,out] list
        Provides the list of test runs to update.

    @param[in] default_phandle
        Provides the PHandle string to apply.
    */
    private static void apply_default_phandle( List<TestRun> list, string default_phandle )
        {
        foreach( TestRun tr in list )
            {
            bool has_phandle = false;
            if( tr.SerialNumberAttributes != null )
                {
                foreach( SerialNumberAttributes attr in tr.SerialNumberAttributes )
                    {
                    if( string.Equals(attr.Name, "PHandle", StringComparison.OrdinalIgnoreCase) )
                        {
                        has_phandle = true;
                        break;
                        }
                    }

                if( !has_phandle )
                    {
                    tr.SerialNumberAttributes.Add(new SerialNumberAttributes
                        {
                        SerialNumberAttributes_Key = 1,
                        Name = "PHandle",
                        Value = default_phandle
                        });
                    }
                }
            }
        }

    /** @brief
        Finds the first non-null, non-whitespace route step across the test run collection.

    @details
        - Iterates sequentially through test runs until a non-empty route step is encountered.
        - Returns null if no valid route step is found.

    @param[in] list
        Provides the list of test runs to search.

    @return
        Returns the first valid route step string, or null if not found.
    */
    private static string find_default_route_step( List<TestRun> list )
        {
        foreach( TestRun tr in list )
            {
            if( !string.IsNullOrWhiteSpace(tr.Routestep)  && tr.Routestep!= "NOT_SET")
                {
                return tr.Routestep;
                }
            }

        return "   ";
        }

    /** @brief
        Applies a default route step to test runs missing a valid route step.

    @details
        - Assigns the default route step string to any test run where the current step is empty or whitespace.

    @param[in,out] list
        Provides the list of test runs to update.

    @param[in] default_route_step
        Provides the default route step string to apply.
    */
    private static void apply_default_route_step( List<TestRun> list, string default_route_step )
        {
        foreach( TestRun tr in list )
            {
            if( string.IsNullOrWhiteSpace(tr.Routestep) || tr.Routestep=="NOT_SET")
                {
                tr.Routestep = default_route_step;
                }
            }
        }
    }
