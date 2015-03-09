using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for SummaryForm.
	/// </summary>
    public class SummaryForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.Label allocatedBytesLabel;
        private System.Windows.Forms.Label allocatedBytesValueLabel;
        private System.Windows.Forms.Label relocatedBytesLabel;
        private System.Windows.Forms.Label relocatedBytesValueLabel;
        private System.Windows.Forms.Label objectsFinalizedLabel;
        private System.Windows.Forms.Label finalHeapBytesLabel;
        private System.Windows.Forms.Label finalHeapBytesValueLabel;
        private System.Windows.Forms.Label objectsFinalizedValueLabel;
        private System.Windows.Forms.Label criticalObjectsFinalizedLabel;
        private System.Windows.Forms.Label criticalObjectsFinalizedValueLabel;
        private System.Windows.Forms.Label gen0CollectionsLabel;
        private System.Windows.Forms.Label gen1CollectionsLabel;
        private System.Windows.Forms.Label gen0CollectionsValueLabel;
        private System.Windows.Forms.Label gen1CollectionsValueLabel;
        private System.Windows.Forms.Label gen2CollectionsLabel;
        private System.Windows.Forms.Label inducedCollectionsLabel;
        private System.Windows.Forms.Label gen2CollectionsValueLabel;
        private System.Windows.Forms.Label gen0HeapBytesLabel;
        private System.Windows.Forms.Label gen0HeapBytesValueLabel;
        private System.Windows.Forms.Label gen1HeapBytesLabel;
        private System.Windows.Forms.Label gen1HeapBytesValueLabel;
        private System.Windows.Forms.Label largeObjectHeapBytesLabel;
        private System.Windows.Forms.Label gen2HeapBytesLabel;
        private System.Windows.Forms.Label largeObjectHeapBytesValueLabel;
        private System.Windows.Forms.Label gen2HeapBytesValueLabel;
        private System.Windows.Forms.Label handlesCreatedLabel;
        private System.Windows.Forms.Label handlesCreatedValueLabel;
        private System.Windows.Forms.Label handlesDestroyedLabel;
        private System.Windows.Forms.Label handlesDestroyedValueLabel;
        private System.Windows.Forms.Label handlesSurvivingLabel;
        private System.Windows.Forms.Label handlesSurvivingValueLabel;
        private System.Windows.Forms.Label heapDumpsLabel;
        private System.Windows.Forms.Label commentsLabel;
        private System.Windows.Forms.Label heapDumpsValueLabel;
        private System.Windows.Forms.Label commentsValueLabel;
        private System.Windows.Forms.Label inducedCollectionsValueLabel;
        private System.Windows.Forms.Button allocationGraphButton;
        private System.Windows.Forms.Button relocatedHistogramButton;
        private System.Windows.Forms.Button finalHeapObjectsByAddressButton;
        private System.Windows.Forms.Button allocatedHistogramButton;
        private System.Windows.Forms.Button timeLineButton;
        private System.Windows.Forms.Button finalHeapHistogramByAgeButton;
        private System.Windows.Forms.Button criticalFinalizedHistogramButton;
        private System.Windows.Forms.Button finalizedHistogramButton;
        private System.Windows.Forms.Button finalHeapHistogramButton;
        private System.Windows.Forms.Button handleAllocationGraphButton;
        private System.Windows.Forms.Button heapGraphButton;
        private System.Windows.Forms.Button commentsButton;
        private System.Windows.Forms.Button survingHandlesAllocationGraphButton;

        private ReadNewLog log;
        private ReadLogResult logResult;
        private string scenario = "";
        private System.Windows.Forms.MenuItem copyMenuItem;
        private System.Windows.Forms.MenuItem contextCopyMenuItem;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private string FormatNumber(double number)
        {
            return string.Format("{0:n0}", number);
        }

        private string CalculateTotalSize(Histogram histogram)
        {
            double totalSize = 0.0;
            for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
            {
                int count = histogram.typeSizeStacktraceToCount[i];
                if (count > 0)
                {
                    int[] stacktrace = histogram.readNewLog.stacktraceTable.IndexToStacktrace(i);
                    int size = stacktrace[1];
                    totalSize += (ulong)size*(ulong)count;
                }
            }
            return FormatNumber(totalSize);
        }

        private string CalculateTotalCount(Histogram histogram)
        {
            double totalCount = 0.0;
            for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
            {
                int count = histogram.typeSizeStacktraceToCount[i];
                totalCount += count;
            }
            return FormatNumber(totalCount);
        }

        private Histogram GetFinalHeapHistogram()
        {
            Histogram histogram = new Histogram(log);
            LiveObjectTable.LiveObject o;
            for (logResult.liveObjectTable.GetNextObject(0, ulong.MaxValue, out o);
                o.id < ulong.MaxValue && o.id + o.size >= o.id;
                logResult.liveObjectTable.GetNextObject(o.id + o.size, ulong.MaxValue, out o))
            {
                histogram.AddObject(o.typeSizeStacktraceIndex, 1);
            }

            return histogram;
        }

        private void FillInNumbers()
        {
            allocatedBytesValueLabel.Text = CalculateTotalSize(logResult.allocatedHistogram);
            relocatedBytesValueLabel.Text = CalculateTotalSize(logResult.relocatedHistogram);
            finalHeapBytesValueLabel.Text = CalculateTotalSize(GetFinalHeapHistogram());

            gen0CollectionsValueLabel.Text = FormatNumber(logResult.liveObjectTable.lastGcGen0Count);
            gen1CollectionsValueLabel.Text = FormatNumber(logResult.liveObjectTable.lastGcGen1Count);
            gen2CollectionsValueLabel.Text = FormatNumber(logResult.liveObjectTable.lastGcGen2Count);
            inducedCollectionsValueLabel.Text = "Unknown";

            gen0HeapBytesValueLabel.Text = "Unknown";
            gen1HeapBytesValueLabel.Text = "Unknown";
            gen2HeapBytesValueLabel.Text = "Unknown";
            objectsFinalizedValueLabel.Text =  "Unknown";
            criticalObjectsFinalizedValueLabel.Text = "Unknown";
            largeObjectHeapBytesValueLabel.Text = "Unknown";
            if (log.gcCount[0] > 0)
            {
                objectsFinalizedValueLabel.Text =  CalculateTotalCount(logResult.finalizerHistogram);
                criticalObjectsFinalizedValueLabel.Text = CalculateTotalCount(logResult.criticalFinalizerHistogram);
                inducedCollectionsValueLabel.Text = FormatNumber(log.inducedGcCount[0]);
                gen0HeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[0] / (uint)log.gcCount[0]);
                if (log.gcCount[1] > 0)
                    gen1HeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[1] / (uint)log.gcCount[1]);
                else
                    gen1HeapBytesValueLabel.Text = FormatNumber(log.generationSize[1]);
                if (log.gcCount[2] > 0)
                    gen2HeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[2] / (uint)log.gcCount[2]);
                else
                    gen2HeapBytesValueLabel.Text = FormatNumber(log.generationSize[2]);
                if (log.gcCount[3] > 0)
                    largeObjectHeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[3] / (uint)log.gcCount[3]);
                else
                    largeObjectHeapBytesValueLabel.Text = FormatNumber(log.generationSize[3]);
            }
            else if (!logResult.createdHandlesHistogram.Empty)
            {
                // we know this is a new format log file
                // log.gcCount[0] was zero because there were no collections
                // in that case we know there were no induced collections and no finalized objects
                inducedCollectionsValueLabel.Text = "0";
                objectsFinalizedValueLabel.Text =  "0";
                criticalObjectsFinalizedValueLabel.Text = "0";
            }

            if (logResult.createdHandlesHistogram.Empty)
            {
                handlesCreatedValueLabel.Text = "Unknown";
                handlesDestroyedValueLabel.Text = "Unknown";
                handlesSurvivingValueLabel.Text = "Unknown";
            }
            else
            {
                handlesCreatedValueLabel.Text = CalculateTotalCount(logResult.createdHandlesHistogram);
                handlesDestroyedValueLabel.Text = CalculateTotalCount(logResult.destroyedHandlesHistogram);
                int count = 0;
                foreach (HandleInfo handleInfo in logResult.handleHash.Values)
                    count++;

                handlesSurvivingValueLabel.Text = FormatNumber(count);
            }

            commentsValueLabel.Text = FormatNumber(log.commentEventList.count);
            heapDumpsValueLabel.Text = FormatNumber(log.heapDumpEventList.count);
        }

        private void EnableDisableButtons()
        {
            allocatedHistogramButton.Enabled = !logResult.allocatedHistogram.Empty;
            allocationGraphButton.Enabled = !logResult.allocatedHistogram.Empty;
            relocatedHistogramButton.Enabled = !logResult.relocatedHistogram.Empty;
            finalizedHistogramButton.Enabled = !logResult.finalizerHistogram.Empty;
            criticalFinalizedHistogramButton.Enabled = !logResult.criticalFinalizerHistogram.Empty;
            handleAllocationGraphButton.Enabled = !logResult.createdHandlesHistogram.Empty;
            survingHandlesAllocationGraphButton.Enabled = logResult.handleHash.Count > 0;
            heapGraphButton.Enabled = log.heapDumpEventList.count > 0;
            commentsButton.Enabled = log.commentEventList.count > 0;
        }

        internal SummaryForm(ReadNewLog log, ReadLogResult logResult, string scenario)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.log = log;
            this.logResult = logResult;
            this.scenario = scenario;
            this.Text = "Summary for " + scenario;

            FillInNumbers();

            EnableDisableButtons();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

		#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.allocatedBytesLabel = new System.Windows.Forms.Label();
            this.allocationGraphButton = new System.Windows.Forms.Button();
            this.relocatedHistogramButton = new System.Windows.Forms.Button();
            this.relocatedBytesLabel = new System.Windows.Forms.Label();
            this.gen0CollectionsLabel = new System.Windows.Forms.Label();
            this.finalHeapObjectsByAddressButton = new System.Windows.Forms.Button();
            this.allocatedHistogramButton = new System.Windows.Forms.Button();
            this.criticalObjectsFinalizedLabel = new System.Windows.Forms.Label();
            this.objectsFinalizedLabel = new System.Windows.Forms.Label();
            this.handlesSurvivingLabel = new System.Windows.Forms.Label();
            this.handlesCreatedLabel = new System.Windows.Forms.Label();
            this.handlesDestroyedLabel = new System.Windows.Forms.Label();
            this.finalHeapBytesLabel = new System.Windows.Forms.Label();
            this.timeLineButton = new System.Windows.Forms.Button();
            this.finalHeapHistogramByAgeButton = new System.Windows.Forms.Button();
            this.gen2CollectionsLabel = new System.Windows.Forms.Label();
            this.gen1CollectionsLabel = new System.Windows.Forms.Label();
            this.inducedCollectionsLabel = new System.Windows.Forms.Label();
            this.gen0HeapBytesLabel = new System.Windows.Forms.Label();
            this.largeObjectHeapBytesLabel = new System.Windows.Forms.Label();
            this.gen2HeapBytesLabel = new System.Windows.Forms.Label();
            this.gen1HeapBytesLabel = new System.Windows.Forms.Label();
            this.heapDumpsLabel = new System.Windows.Forms.Label();
            this.commentsLabel = new System.Windows.Forms.Label();
            this.heapGraphButton = new System.Windows.Forms.Button();
            this.commentsButton = new System.Windows.Forms.Button();
            this.criticalFinalizedHistogramButton = new System.Windows.Forms.Button();
            this.finalizedHistogramButton = new System.Windows.Forms.Button();
            this.finalHeapHistogramButton = new System.Windows.Forms.Button();
            this.survingHandlesAllocationGraphButton = new System.Windows.Forms.Button();
            this.handleAllocationGraphButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.criticalObjectsFinalizedValueLabel = new System.Windows.Forms.Label();
            this.objectsFinalizedValueLabel = new System.Windows.Forms.Label();
            this.finalHeapBytesValueLabel = new System.Windows.Forms.Label();
            this.relocatedBytesValueLabel = new System.Windows.Forms.Label();
            this.allocatedBytesValueLabel = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.gen1CollectionsValueLabel = new System.Windows.Forms.Label();
            this.inducedCollectionsValueLabel = new System.Windows.Forms.Label();
            this.gen2CollectionsValueLabel = new System.Windows.Forms.Label();
            this.gen0CollectionsValueLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.gen2HeapBytesValueLabel = new System.Windows.Forms.Label();
            this.gen1HeapBytesValueLabel = new System.Windows.Forms.Label();
            this.largeObjectHeapBytesValueLabel = new System.Windows.Forms.Label();
            this.gen0HeapBytesValueLabel = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.handlesSurvivingValueLabel = new System.Windows.Forms.Label();
            this.handlesDestroyedValueLabel = new System.Windows.Forms.Label();
            this.handlesCreatedValueLabel = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.commentsValueLabel = new System.Windows.Forms.Label();
            this.heapDumpsValueLabel = new System.Windows.Forms.Label();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.copyMenuItem = new System.Windows.Forms.MenuItem();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.contextCopyMenuItem = new System.Windows.Forms.MenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // allocatedBytesLabel
            // 
            this.allocatedBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.allocatedBytesLabel.Location = new System.Drawing.Point(24, 24);
            this.allocatedBytesLabel.Name = "allocatedBytesLabel";
            this.allocatedBytesLabel.Size = new System.Drawing.Size(152, 16);
            this.allocatedBytesLabel.TabIndex = 1;
            this.allocatedBytesLabel.Text = "Allocated bytes:";
            // 
            // allocationGraphButton
            // 
            this.allocationGraphButton.Location = new System.Drawing.Point(424, 16);
            this.allocationGraphButton.Name = "allocationGraphButton";
            this.allocationGraphButton.Size = new System.Drawing.Size(128, 23);
            this.allocationGraphButton.TabIndex = 2;
            this.allocationGraphButton.Text = "Allocation Graph";
            this.allocationGraphButton.Click += new System.EventHandler(this.allocationGraphButton_Click);
            // 
            // relocatedHistogramButton
            // 
            this.relocatedHistogramButton.Location = new System.Drawing.Point(288, 40);
            this.relocatedHistogramButton.Name = "relocatedHistogramButton";
            this.relocatedHistogramButton.Size = new System.Drawing.Size(112, 23);
            this.relocatedHistogramButton.TabIndex = 3;
            this.relocatedHistogramButton.Text = "Histogram";
            this.relocatedHistogramButton.Click += new System.EventHandler(this.relocatedHistogramButton_Click);
            // 
            // relocatedBytesLabel
            // 
            this.relocatedBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.relocatedBytesLabel.Location = new System.Drawing.Point(24, 48);
            this.relocatedBytesLabel.Name = "relocatedBytesLabel";
            this.relocatedBytesLabel.Size = new System.Drawing.Size(152, 16);
            this.relocatedBytesLabel.TabIndex = 4;
            this.relocatedBytesLabel.Text = "Relocated bytes: ";
            // 
            // gen0CollectionsLabel
            // 
            this.gen0CollectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen0CollectionsLabel.Location = new System.Drawing.Point(24, 24);
            this.gen0CollectionsLabel.Name = "gen0CollectionsLabel";
            this.gen0CollectionsLabel.Size = new System.Drawing.Size(152, 16);
            this.gen0CollectionsLabel.TabIndex = 6;
            this.gen0CollectionsLabel.Text = "Gen 0 collections:";
            // 
            // finalHeapObjectsByAddressButton
            // 
            this.finalHeapObjectsByAddressButton.Location = new System.Drawing.Point(592, 64);
            this.finalHeapObjectsByAddressButton.Name = "finalHeapObjectsByAddressButton";
            this.finalHeapObjectsByAddressButton.Size = new System.Drawing.Size(136, 23);
            this.finalHeapObjectsByAddressButton.TabIndex = 11;
            this.finalHeapObjectsByAddressButton.Text = "Objects by Address";
            this.finalHeapObjectsByAddressButton.Click += new System.EventHandler(this.finalHeapObjectsByAddressButton_Click);
            // 
            // allocatedHistogramButton
            // 
            this.allocatedHistogramButton.Location = new System.Drawing.Point(288, 16);
            this.allocatedHistogramButton.Name = "allocatedHistogramButton";
            this.allocatedHistogramButton.Size = new System.Drawing.Size(112, 23);
            this.allocatedHistogramButton.TabIndex = 13;
            this.allocatedHistogramButton.Text = "Histogram";
            this.allocatedHistogramButton.Click += new System.EventHandler(this.allocatedHistogramButton_Click);
            // 
            // criticalObjectsFinalizedLabel
            // 
            this.criticalObjectsFinalizedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.criticalObjectsFinalizedLabel.Location = new System.Drawing.Point(24, 120);
            this.criticalObjectsFinalizedLabel.Name = "criticalObjectsFinalizedLabel";
            this.criticalObjectsFinalizedLabel.Size = new System.Drawing.Size(152, 16);
            this.criticalObjectsFinalizedLabel.TabIndex = 14;
            this.criticalObjectsFinalizedLabel.Text = "Critical objects finalized:";
            // 
            // objectsFinalizedLabel
            // 
            this.objectsFinalizedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.objectsFinalizedLabel.Location = new System.Drawing.Point(24, 96);
            this.objectsFinalizedLabel.Name = "objectsFinalizedLabel";
            this.objectsFinalizedLabel.Size = new System.Drawing.Size(152, 16);
            this.objectsFinalizedLabel.TabIndex = 17;
            this.objectsFinalizedLabel.Text = "Objects finalized:";
            // 
            // handlesSurvivingLabel
            // 
            this.handlesSurvivingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.handlesSurvivingLabel.Location = new System.Drawing.Point(24, 72);
            this.handlesSurvivingLabel.Name = "handlesSurvivingLabel";
            this.handlesSurvivingLabel.Size = new System.Drawing.Size(152, 16);
            this.handlesSurvivingLabel.TabIndex = 18;
            this.handlesSurvivingLabel.Text = "Handles surviving:";
            // 
            // handlesCreatedLabel
            // 
            this.handlesCreatedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.handlesCreatedLabel.Location = new System.Drawing.Point(24, 24);
            this.handlesCreatedLabel.Name = "handlesCreatedLabel";
            this.handlesCreatedLabel.Size = new System.Drawing.Size(152, 16);
            this.handlesCreatedLabel.TabIndex = 19;
            this.handlesCreatedLabel.Text = "Handles created:";
            // 
            // handlesDestroyedLabel
            // 
            this.handlesDestroyedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.handlesDestroyedLabel.Location = new System.Drawing.Point(24, 48);
            this.handlesDestroyedLabel.Name = "handlesDestroyedLabel";
            this.handlesDestroyedLabel.Size = new System.Drawing.Size(152, 16);
            this.handlesDestroyedLabel.TabIndex = 20;
            this.handlesDestroyedLabel.Text = "Handles destroyed:";
            // 
            // finalHeapBytesLabel
            // 
            this.finalHeapBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.finalHeapBytesLabel.Location = new System.Drawing.Point(24, 72);
            this.finalHeapBytesLabel.Name = "finalHeapBytesLabel";
            this.finalHeapBytesLabel.Size = new System.Drawing.Size(152, 16);
            this.finalHeapBytesLabel.TabIndex = 5;
            this.finalHeapBytesLabel.Text = "Final Heap bytes: ";
            // 
            // timeLineButton
            // 
            this.timeLineButton.Location = new System.Drawing.Point(288, 16);
            this.timeLineButton.Name = "timeLineButton";
            this.timeLineButton.Size = new System.Drawing.Size(112, 23);
            this.timeLineButton.TabIndex = 26;
            this.timeLineButton.Text = "Time Line";
            this.timeLineButton.Click += new System.EventHandler(this.timeLineButton_Click);
            // 
            // finalHeapHistogramByAgeButton
            // 
            this.finalHeapHistogramByAgeButton.Location = new System.Drawing.Point(424, 64);
            this.finalHeapHistogramByAgeButton.Name = "finalHeapHistogramByAgeButton";
            this.finalHeapHistogramByAgeButton.Size = new System.Drawing.Size(128, 23);
            this.finalHeapHistogramByAgeButton.TabIndex = 28;
            this.finalHeapHistogramByAgeButton.Text = "Histogram by Age";
            this.finalHeapHistogramByAgeButton.Click += new System.EventHandler(this.finalHeapHistogramByAgeButton_Click);
            // 
            // gen2CollectionsLabel
            // 
            this.gen2CollectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen2CollectionsLabel.Location = new System.Drawing.Point(24, 72);
            this.gen2CollectionsLabel.Name = "gen2CollectionsLabel";
            this.gen2CollectionsLabel.Size = new System.Drawing.Size(152, 16);
            this.gen2CollectionsLabel.TabIndex = 29;
            this.gen2CollectionsLabel.Text = "Gen 2 collections:";
            // 
            // gen1CollectionsLabel
            // 
            this.gen1CollectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen1CollectionsLabel.Location = new System.Drawing.Point(24, 48);
            this.gen1CollectionsLabel.Name = "gen1CollectionsLabel";
            this.gen1CollectionsLabel.Size = new System.Drawing.Size(152, 16);
            this.gen1CollectionsLabel.TabIndex = 30;
            this.gen1CollectionsLabel.Text = "Gen 1 collections:";
            // 
            // inducedCollectionsLabel
            // 
            this.inducedCollectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.inducedCollectionsLabel.Location = new System.Drawing.Point(24, 96);
            this.inducedCollectionsLabel.Name = "inducedCollectionsLabel";
            this.inducedCollectionsLabel.Size = new System.Drawing.Size(152, 16);
            this.inducedCollectionsLabel.TabIndex = 31;
            this.inducedCollectionsLabel.Text = "Induced collections:";
            // 
            // gen0HeapBytesLabel
            // 
            this.gen0HeapBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen0HeapBytesLabel.Location = new System.Drawing.Point(24, 24);
            this.gen0HeapBytesLabel.Name = "gen0HeapBytesLabel";
            this.gen0HeapBytesLabel.Size = new System.Drawing.Size(152, 16);
            this.gen0HeapBytesLabel.TabIndex = 35;
            this.gen0HeapBytesLabel.Text = "Gen 0 Heap bytes:";
            // 
            // largeObjectHeapBytesLabel
            // 
            this.largeObjectHeapBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.largeObjectHeapBytesLabel.Location = new System.Drawing.Point(24, 96);
            this.largeObjectHeapBytesLabel.Name = "largeObjectHeapBytesLabel";
            this.largeObjectHeapBytesLabel.Size = new System.Drawing.Size(160, 16);
            this.largeObjectHeapBytesLabel.TabIndex = 36;
            this.largeObjectHeapBytesLabel.Text = "Large Object Heap bytes:";
            // 
            // gen2HeapBytesLabel
            // 
            this.gen2HeapBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen2HeapBytesLabel.Location = new System.Drawing.Point(24, 72);
            this.gen2HeapBytesLabel.Name = "gen2HeapBytesLabel";
            this.gen2HeapBytesLabel.Size = new System.Drawing.Size(152, 16);
            this.gen2HeapBytesLabel.TabIndex = 37;
            this.gen2HeapBytesLabel.Text = "Gen 2 Heap bytes:";
            // 
            // gen1HeapBytesLabel
            // 
            this.gen1HeapBytesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen1HeapBytesLabel.Location = new System.Drawing.Point(24, 48);
            this.gen1HeapBytesLabel.Name = "gen1HeapBytesLabel";
            this.gen1HeapBytesLabel.Size = new System.Drawing.Size(152, 16);
            this.gen1HeapBytesLabel.TabIndex = 38;
            this.gen1HeapBytesLabel.Text = "Gen 1 Heap bytes:";
            // 
            // heapDumpsLabel
            // 
            this.heapDumpsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.heapDumpsLabel.Location = new System.Drawing.Point(24, 24);
            this.heapDumpsLabel.Name = "heapDumpsLabel";
            this.heapDumpsLabel.Size = new System.Drawing.Size(96, 16);
            this.heapDumpsLabel.TabIndex = 39;
            this.heapDumpsLabel.Text = "Heap Dumps:";
            // 
            // commentsLabel
            // 
            this.commentsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.commentsLabel.Location = new System.Drawing.Point(24, 48);
            this.commentsLabel.Name = "commentsLabel";
            this.commentsLabel.Size = new System.Drawing.Size(96, 16);
            this.commentsLabel.TabIndex = 40;
            this.commentsLabel.Text = "Comments:";
            // 
            // heapGraphButton
            // 
            this.heapGraphButton.Location = new System.Drawing.Point(224, 16);
            this.heapGraphButton.Name = "heapGraphButton";
            this.heapGraphButton.Size = new System.Drawing.Size(112, 23);
            this.heapGraphButton.TabIndex = 41;
            this.heapGraphButton.Text = "Heap Graph";
            this.heapGraphButton.Click += new System.EventHandler(this.heapGraphButton_Click);
            // 
            // commentsButton
            // 
            this.commentsButton.Location = new System.Drawing.Point(224, 40);
            this.commentsButton.Name = "commentsButton";
            this.commentsButton.Size = new System.Drawing.Size(112, 23);
            this.commentsButton.TabIndex = 42;
            this.commentsButton.Text = "Comments";
            this.commentsButton.Click += new System.EventHandler(this.commentsButton_Click);
            // 
            // criticalFinalizedHistogramButton
            // 
            this.criticalFinalizedHistogramButton.Location = new System.Drawing.Point(288, 112);
            this.criticalFinalizedHistogramButton.Name = "criticalFinalizedHistogramButton";
            this.criticalFinalizedHistogramButton.Size = new System.Drawing.Size(112, 23);
            this.criticalFinalizedHistogramButton.TabIndex = 46;
            this.criticalFinalizedHistogramButton.Text = "Histogram";
            this.criticalFinalizedHistogramButton.Click += new System.EventHandler(this.criticalFinalizedHistogramButton_Click);
            // 
            // finalizedHistogramButton
            // 
            this.finalizedHistogramButton.Location = new System.Drawing.Point(288, 88);
            this.finalizedHistogramButton.Name = "finalizedHistogramButton";
            this.finalizedHistogramButton.Size = new System.Drawing.Size(112, 23);
            this.finalizedHistogramButton.TabIndex = 47;
            this.finalizedHistogramButton.Text = "Histogram";
            this.finalizedHistogramButton.Click += new System.EventHandler(this.finalizedHistogramButton_Click);
            // 
            // finalHeapHistogramButton
            // 
            this.finalHeapHistogramButton.Location = new System.Drawing.Point(288, 64);
            this.finalHeapHistogramButton.Name = "finalHeapHistogramButton";
            this.finalHeapHistogramButton.Size = new System.Drawing.Size(112, 23);
            this.finalHeapHistogramButton.TabIndex = 48;
            this.finalHeapHistogramButton.Text = "Histogram";
            this.finalHeapHistogramButton.Click += new System.EventHandler(this.finalHeapHistogramButton_Click);
            // 
            // survingHandlesAllocationGraphButton
            // 
            this.survingHandlesAllocationGraphButton.Location = new System.Drawing.Point(288, 64);
            this.survingHandlesAllocationGraphButton.Name = "survingHandlesAllocationGraphButton";
            this.survingHandlesAllocationGraphButton.Size = new System.Drawing.Size(120, 23);
            this.survingHandlesAllocationGraphButton.TabIndex = 49;
            this.survingHandlesAllocationGraphButton.Text = "Allocation Graph";
            this.survingHandlesAllocationGraphButton.Click += new System.EventHandler(this.survingHandlesAllocationGraphButton_Click);
            // 
            // handleAllocationGraphButton
            // 
            this.handleAllocationGraphButton.Location = new System.Drawing.Point(288, 16);
            this.handleAllocationGraphButton.Name = "handleAllocationGraphButton";
            this.handleAllocationGraphButton.Size = new System.Drawing.Size(120, 23);
            this.handleAllocationGraphButton.TabIndex = 50;
            this.handleAllocationGraphButton.Text = "Allocation Graph";
            this.handleAllocationGraphButton.Click += new System.EventHandler(this.handleAllocationGraphButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.criticalObjectsFinalizedValueLabel);
            this.groupBox1.Controls.Add(this.objectsFinalizedValueLabel);
            this.groupBox1.Controls.Add(this.finalHeapBytesValueLabel);
            this.groupBox1.Controls.Add(this.relocatedBytesValueLabel);
            this.groupBox1.Controls.Add(this.allocatedBytesLabel);
            this.groupBox1.Controls.Add(this.relocatedBytesLabel);
            this.groupBox1.Controls.Add(this.finalHeapBytesLabel);
            this.groupBox1.Controls.Add(this.objectsFinalizedLabel);
            this.groupBox1.Controls.Add(this.criticalObjectsFinalizedLabel);
            this.groupBox1.Controls.Add(this.relocatedHistogramButton);
            this.groupBox1.Controls.Add(this.allocatedHistogramButton);
            this.groupBox1.Controls.Add(this.finalHeapHistogramButton);
            this.groupBox1.Controls.Add(this.finalizedHistogramButton);
            this.groupBox1.Controls.Add(this.criticalFinalizedHistogramButton);
            this.groupBox1.Controls.Add(this.finalHeapHistogramByAgeButton);
            this.groupBox1.Controls.Add(this.finalHeapObjectsByAddressButton);
            this.groupBox1.Controls.Add(this.allocationGraphButton);
            this.groupBox1.Controls.Add(this.allocatedBytesValueLabel);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(32, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 144);
            this.groupBox1.TabIndex = 51;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Heap Statistics";
            // 
            // criticalObjectsFinalizedValueLabel
            // 
            this.criticalObjectsFinalizedValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.criticalObjectsFinalizedValueLabel.Location = new System.Drawing.Point(176, 120);
            this.criticalObjectsFinalizedValueLabel.Name = "criticalObjectsFinalizedValueLabel";
            this.criticalObjectsFinalizedValueLabel.Size = new System.Drawing.Size(96, 16);
            this.criticalObjectsFinalizedValueLabel.TabIndex = 56;
            this.criticalObjectsFinalizedValueLabel.Text = "4,123,456,789";
            this.criticalObjectsFinalizedValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // objectsFinalizedValueLabel
            // 
            this.objectsFinalizedValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.objectsFinalizedValueLabel.Location = new System.Drawing.Point(176, 96);
            this.objectsFinalizedValueLabel.Name = "objectsFinalizedValueLabel";
            this.objectsFinalizedValueLabel.Size = new System.Drawing.Size(96, 16);
            this.objectsFinalizedValueLabel.TabIndex = 55;
            this.objectsFinalizedValueLabel.Text = "4,123,456,789";
            this.objectsFinalizedValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // finalHeapBytesValueLabel
            // 
            this.finalHeapBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.finalHeapBytesValueLabel.Location = new System.Drawing.Point(176, 72);
            this.finalHeapBytesValueLabel.Name = "finalHeapBytesValueLabel";
            this.finalHeapBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.finalHeapBytesValueLabel.TabIndex = 54;
            this.finalHeapBytesValueLabel.Text = "4,123,456,789";
            this.finalHeapBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // relocatedBytesValueLabel
            // 
            this.relocatedBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.relocatedBytesValueLabel.Location = new System.Drawing.Point(176, 48);
            this.relocatedBytesValueLabel.Name = "relocatedBytesValueLabel";
            this.relocatedBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.relocatedBytesValueLabel.TabIndex = 53;
            this.relocatedBytesValueLabel.Text = "4,123,456,789";
            this.relocatedBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // allocatedBytesValueLabel
            // 
            this.allocatedBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.allocatedBytesValueLabel.Location = new System.Drawing.Point(176, 24);
            this.allocatedBytesValueLabel.Name = "allocatedBytesValueLabel";
            this.allocatedBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.allocatedBytesValueLabel.TabIndex = 52;
            this.allocatedBytesValueLabel.Text = "4,123,456,789";
            this.allocatedBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.gen1CollectionsValueLabel);
            this.groupBox2.Controls.Add(this.inducedCollectionsValueLabel);
            this.groupBox2.Controls.Add(this.gen2CollectionsValueLabel);
            this.groupBox2.Controls.Add(this.gen0CollectionsLabel);
            this.groupBox2.Controls.Add(this.gen1CollectionsLabel);
            this.groupBox2.Controls.Add(this.gen2CollectionsLabel);
            this.groupBox2.Controls.Add(this.inducedCollectionsLabel);
            this.groupBox2.Controls.Add(this.timeLineButton);
            this.groupBox2.Controls.Add(this.gen0CollectionsValueLabel);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(32, 192);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(424, 128);
            this.groupBox2.TabIndex = 52;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Garbage Collection Statistics";
            // 
            // gen1CollectionsValueLabel
            // 
            this.gen1CollectionsValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen1CollectionsValueLabel.Location = new System.Drawing.Point(176, 48);
            this.gen1CollectionsValueLabel.Name = "gen1CollectionsValueLabel";
            this.gen1CollectionsValueLabel.Size = new System.Drawing.Size(96, 16);
            this.gen1CollectionsValueLabel.TabIndex = 60;
            this.gen1CollectionsValueLabel.Text = "4,123,456,789";
            this.gen1CollectionsValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inducedCollectionsValueLabel
            // 
            this.inducedCollectionsValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.inducedCollectionsValueLabel.Location = new System.Drawing.Point(176, 96);
            this.inducedCollectionsValueLabel.Name = "inducedCollectionsValueLabel";
            this.inducedCollectionsValueLabel.Size = new System.Drawing.Size(96, 16);
            this.inducedCollectionsValueLabel.TabIndex = 59;
            this.inducedCollectionsValueLabel.Text = "4,123,456,789";
            this.inducedCollectionsValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // gen2CollectionsValueLabel
            // 
            this.gen2CollectionsValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen2CollectionsValueLabel.Location = new System.Drawing.Point(176, 72);
            this.gen2CollectionsValueLabel.Name = "gen2CollectionsValueLabel";
            this.gen2CollectionsValueLabel.Size = new System.Drawing.Size(96, 16);
            this.gen2CollectionsValueLabel.TabIndex = 58;
            this.gen2CollectionsValueLabel.Text = "4,123,456,789";
            this.gen2CollectionsValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // gen0CollectionsValueLabel
            // 
            this.gen0CollectionsValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen0CollectionsValueLabel.Location = new System.Drawing.Point(176, 24);
            this.gen0CollectionsValueLabel.Name = "gen0CollectionsValueLabel";
            this.gen0CollectionsValueLabel.Size = new System.Drawing.Size(96, 16);
            this.gen0CollectionsValueLabel.TabIndex = 57;
            this.gen0CollectionsValueLabel.Text = "4,123,456,789";
            this.gen0CollectionsValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.gen2HeapBytesValueLabel);
            this.groupBox3.Controls.Add(this.gen1HeapBytesValueLabel);
            this.groupBox3.Controls.Add(this.largeObjectHeapBytesValueLabel);
            this.groupBox3.Controls.Add(this.gen0HeapBytesValueLabel);
            this.groupBox3.Controls.Add(this.gen0HeapBytesLabel);
            this.groupBox3.Controls.Add(this.gen1HeapBytesLabel);
            this.groupBox3.Controls.Add(this.gen2HeapBytesLabel);
            this.groupBox3.Controls.Add(this.largeObjectHeapBytesLabel);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(480, 192);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(352, 128);
            this.groupBox3.TabIndex = 53;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Garbage Collector Generation Sizes";
            // 
            // gen2HeapBytesValueLabel
            // 
            this.gen2HeapBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen2HeapBytesValueLabel.Location = new System.Drawing.Point(208, 72);
            this.gen2HeapBytesValueLabel.Name = "gen2HeapBytesValueLabel";
            this.gen2HeapBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.gen2HeapBytesValueLabel.TabIndex = 61;
            this.gen2HeapBytesValueLabel.Text = "4,123,456,789";
            this.gen2HeapBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // gen1HeapBytesValueLabel
            // 
            this.gen1HeapBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen1HeapBytesValueLabel.Location = new System.Drawing.Point(208, 48);
            this.gen1HeapBytesValueLabel.Name = "gen1HeapBytesValueLabel";
            this.gen1HeapBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.gen1HeapBytesValueLabel.TabIndex = 60;
            this.gen1HeapBytesValueLabel.Text = "4,123,456,789";
            this.gen1HeapBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // largeObjectHeapBytesValueLabel
            // 
            this.largeObjectHeapBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.largeObjectHeapBytesValueLabel.Location = new System.Drawing.Point(208, 96);
            this.largeObjectHeapBytesValueLabel.Name = "largeObjectHeapBytesValueLabel";
            this.largeObjectHeapBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.largeObjectHeapBytesValueLabel.TabIndex = 59;
            this.largeObjectHeapBytesValueLabel.Text = "4,123,456,789";
            this.largeObjectHeapBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // gen0HeapBytesValueLabel
            // 
            this.gen0HeapBytesValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.gen0HeapBytesValueLabel.Location = new System.Drawing.Point(208, 24);
            this.gen0HeapBytesValueLabel.Name = "gen0HeapBytesValueLabel";
            this.gen0HeapBytesValueLabel.Size = new System.Drawing.Size(96, 16);
            this.gen0HeapBytesValueLabel.TabIndex = 58;
            this.gen0HeapBytesValueLabel.Text = "4,123,456,789";
            this.gen0HeapBytesValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.handlesSurvivingValueLabel);
            this.groupBox4.Controls.Add(this.handlesDestroyedValueLabel);
            this.groupBox4.Controls.Add(this.handlesCreatedValueLabel);
            this.groupBox4.Controls.Add(this.handlesCreatedLabel);
            this.groupBox4.Controls.Add(this.handlesDestroyedLabel);
            this.groupBox4.Controls.Add(this.handlesSurvivingLabel);
            this.groupBox4.Controls.Add(this.handleAllocationGraphButton);
            this.groupBox4.Controls.Add(this.survingHandlesAllocationGraphButton);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(32, 344);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(424, 100);
            this.groupBox4.TabIndex = 54;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "GC Handle Statistics";
            // 
            // handlesSurvivingValueLabel
            // 
            this.handlesSurvivingValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.handlesSurvivingValueLabel.Location = new System.Drawing.Point(176, 72);
            this.handlesSurvivingValueLabel.Name = "handlesSurvivingValueLabel";
            this.handlesSurvivingValueLabel.Size = new System.Drawing.Size(96, 16);
            this.handlesSurvivingValueLabel.TabIndex = 62;
            this.handlesSurvivingValueLabel.Text = "4,123,456,789";
            this.handlesSurvivingValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // handlesDestroyedValueLabel
            // 
            this.handlesDestroyedValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.handlesDestroyedValueLabel.Location = new System.Drawing.Point(176, 48);
            this.handlesDestroyedValueLabel.Name = "handlesDestroyedValueLabel";
            this.handlesDestroyedValueLabel.Size = new System.Drawing.Size(96, 16);
            this.handlesDestroyedValueLabel.TabIndex = 61;
            this.handlesDestroyedValueLabel.Text = "4,123,456,789";
            this.handlesDestroyedValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // handlesCreatedValueLabel
            // 
            this.handlesCreatedValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.handlesCreatedValueLabel.Location = new System.Drawing.Point(176, 24);
            this.handlesCreatedValueLabel.Name = "handlesCreatedValueLabel";
            this.handlesCreatedValueLabel.Size = new System.Drawing.Size(96, 16);
            this.handlesCreatedValueLabel.TabIndex = 60;
            this.handlesCreatedValueLabel.Text = "4,123,456,789";
            this.handlesCreatedValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.commentsValueLabel);
            this.groupBox5.Controls.Add(this.heapDumpsValueLabel);
            this.groupBox5.Controls.Add(this.heapDumpsLabel);
            this.groupBox5.Controls.Add(this.commentsLabel);
            this.groupBox5.Controls.Add(this.heapGraphButton);
            this.groupBox5.Controls.Add(this.commentsButton);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(480, 344);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(352, 100);
            this.groupBox5.TabIndex = 55;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Profiling Statistics";
            // 
            // commentsValueLabel
            // 
            this.commentsValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.commentsValueLabel.Location = new System.Drawing.Point(120, 48);
            this.commentsValueLabel.Name = "commentsValueLabel";
            this.commentsValueLabel.Size = new System.Drawing.Size(96, 16);
            this.commentsValueLabel.TabIndex = 61;
            this.commentsValueLabel.Text = "4,123,456,789";
            this.commentsValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // heapDumpsValueLabel
            // 
            this.heapDumpsValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.heapDumpsValueLabel.Location = new System.Drawing.Point(120, 24);
            this.heapDumpsValueLabel.Name = "heapDumpsValueLabel";
            this.heapDumpsValueLabel.Size = new System.Drawing.Size(96, 16);
            this.heapDumpsValueLabel.TabIndex = 60;
            this.heapDumpsValueLabel.Text = "4,123,456,789";
            this.heapDumpsValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.copyMenuItem});
            this.menuItem1.Text = "Edit";
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Index = 0;
            this.copyMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.copyMenuItem.Text = "Copy";
            this.copyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                         this.contextCopyMenuItem});
            // 
            // contextCopyMenuItem
            // 
            this.contextCopyMenuItem.Index = 0;
            this.contextCopyMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.contextCopyMenuItem.Text = "Copy";
            this.contextCopyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // SummaryForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(864, 477);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Menu = this.mainMenu1;
            this.Name = "SummaryForm";
            this.Text = "Summary";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion


        private void allocatedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Allocated Objects for: " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.allocatedHistogram, title);
            histogramViewForm.Show();
        }

        private void allocationGraphButton_Click(object sender, System.EventArgs e)
        {
            Graph graph = logResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
            graph.graphType = Graph.GraphType.AllocationGraph;
            string title = "Allocation Graph for: " + scenario;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Show();
        }

        private void relocatedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Relocated Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.relocatedHistogram, title);
            histogramViewForm.Show();        
        }

        private void finalHeapHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Surviving Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(GetFinalHeapHistogram(), title);
            histogramViewForm.Show();
        }

        private void finalHeapHistogramByAgeButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Age for Live Objects for " + scenario;
            AgeHistogram ageHistogram = new AgeHistogram(logResult.liveObjectTable, title);
            ageHistogram.Show();
        }

        private void finalHeapObjectsByAddressButton_Click(object sender, System.EventArgs e)
        {
            ViewByAddressForm viewByAddressForm = new ViewByAddressForm();
            viewByAddressForm.Visible = true;
        }

        private void finalizedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Finalized Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.finalizerHistogram, title);
            histogramViewForm.Show();
        }

        private void criticalFinalizedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Finalized Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.criticalFinalizerHistogram, title);
            histogramViewForm.Show();        
        }

        private void timeLineButton_Click(object sender, System.EventArgs e)
        {
            TimeLineViewForm timeLineViewForm = new TimeLineViewForm();
            timeLineViewForm.Visible = true;
        }

        private void heapGraphButton_Click(object sender, System.EventArgs e)
        {
            Graph graph = logResult.objectGraph.BuildTypeGraph(new FilterForm());
            string title = "Heap Graph for " + scenario;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Show();
        }

        private void commentsButton_Click(object sender, System.EventArgs e)
        {
            ViewCommentsForm viewCommentsForm = new ViewCommentsForm(log);
            viewCommentsForm.Visible = true;
        }

        private void CreateHandleAllocationGraph(Histogram histogram, string title)
        {
            Graph graph = histogram.BuildHandleAllocationGraph(new FilterForm());
            graph.graphType = Graph.GraphType.HandleAllocationGraph;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Show();        
        }

        private void handleAllocationGraphButton_Click(object sender, System.EventArgs e)
        {
            string title = "Handle Allocation Graph for: " + scenario;
            CreateHandleAllocationGraph(logResult.createdHandlesHistogram, title);
        }

        private void survingHandlesAllocationGraphButton_Click(object sender, System.EventArgs e)
        {
            Histogram histogram = new Histogram(log);
            foreach (HandleInfo handleInfo in logResult.handleHash.Values)
            {   
                histogram.AddObject(handleInfo.allocStacktraceId, 1);
            }
            string title = "Surviving Handle Allocation Graph for: " + scenario;
            CreateHandleAllocationGraph(histogram, title);
        }
        
        private void copyMenuItem_Click(object sender, System.EventArgs e)
        {
            Label[] copyLabel = new Label []
            {
                allocatedBytesLabel,           allocatedBytesValueLabel,
                relocatedBytesLabel,           relocatedBytesValueLabel,
                finalHeapBytesLabel,           finalHeapBytesValueLabel,
                objectsFinalizedLabel,         objectsFinalizedValueLabel,
                criticalObjectsFinalizedLabel, criticalObjectsFinalizedValueLabel,
                gen0CollectionsLabel,          gen0CollectionsValueLabel,
                gen1CollectionsLabel,          gen1CollectionsValueLabel,
                gen2CollectionsLabel,          gen2CollectionsValueLabel,
                inducedCollectionsLabel,       inducedCollectionsValueLabel,
                gen0HeapBytesLabel,            gen0HeapBytesValueLabel,
                gen1HeapBytesLabel,            gen1HeapBytesValueLabel,
                gen2HeapBytesLabel,            gen2HeapBytesValueLabel,
                largeObjectHeapBytesLabel,     largeObjectHeapBytesValueLabel,
                handlesCreatedLabel,           handlesCreatedValueLabel,
                handlesDestroyedLabel,         handlesDestroyedValueLabel,
                handlesSurvivingLabel,         handlesSurvivingValueLabel,
                heapDumpsLabel,                heapDumpsValueLabel,
                commentsLabel,                 commentsValueLabel,
            };

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Summary for {0}\r\n", scenario);
            for (int i = 0; i < copyLabel.Length; i += 2)
                sb.AppendFormat("{0,-30}{1,13}\r\n", copyLabel[i].Text, copyLabel[i+1].Text);
            Clipboard.SetDataObject(sb.ToString());
        }
    }
}
