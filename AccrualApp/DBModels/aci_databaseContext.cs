using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AccrualApp.DBModels
{
    public partial class aci_databaseContext : DbContext
    {
        public aci_databaseContext()
        {
        }

        public aci_databaseContext(DbContextOptions<aci_databaseContext> options)
            : base(options)
        {
        }



        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<AccountType> AccountType { get; set; }
        public virtual DbSet<AcibalanceSheet> AcibalanceSheet { get; set; }
        public virtual DbSet<Acibudget> Acibudget { get; set; }
        public virtual DbSet<AcicompanyMaster> AcicompanyMaster { get; set; }
        public virtual DbSet<AcicustomerMaster> AcicustomerMaster { get; set; }
        public virtual DbSet<Acidescretionary> Acidescretionary { get; set; }
        public virtual DbSet<Acidistributor> Acidistributor { get; set; }
        public virtual DbSet<Acihr> Acihr { get; set; }
        public virtual DbSet<AciindependentContractTask> AciindependentContractTask { get; set; }
        public virtual DbSet<AciitemMaster> AciitemMaster { get; set; }
        public virtual DbSet<AcikeyInfo> AcikeyInfo { get; set; }
        public virtual DbSet<AcimonthlyExpense> AcimonthlyExpense { get; set; }
        public virtual DbSet<Aciprojection> Aciprojection { get; set; }
        public virtual DbSet<Acipublisher> Acipublisher { get; set; }
        public virtual DbSet<AgingDate> AgingDate { get; set; }
        public virtual DbSet<BsDate> BsDate { get; set; }
        public virtual DbSet<Budget> Budget { get; set; }
        public virtual DbSet<CashFlowProjection> CashFlowProjection { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<Ebitda> Ebitda { get; set; }
        public virtual DbSet<IpaData> IpaData { get; set; }
        public virtual DbSet<Projection> Projection { get; set; }
        public virtual DbSet<ProjectionMonthly> ProjectionMonthly { get; set; }
        public virtual DbSet<QbDate> QbDate { get; set; }
        public virtual DbSet<Region> Region { get; set; }
        public virtual DbSet<Transaction> Transaction { get; set; }
        public virtual DbSet<TransactionTmp> TransactionTmp { get; set; }
        public virtual DbSet<Vendor> Vendor { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=tcp:aci-database-server.database.windows.net,1433;Database=aci_database;User ID=cslabs-admin;Password=Labs@CS#1192;Encrypt=True;TrustServerCertificate=False;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.HasIndex(e => e.AccountTypeId)
                    .HasName("index_account_type_id");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.AccountName)
                    .IsRequired()
                    .HasColumnName("account_name")
                    .HasMaxLength(250);

                entity.Property(e => e.AccountNum)
                    .IsRequired()
                    .HasColumnName("account_num")
                    .HasMaxLength(50);

                entity.Property(e => e.AccountTypeId).HasColumnName("account_type_id");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.HasOne(d => d.AccountType)
                    .WithMany(p => p.Account)
                    .HasForeignKey(d => d.AccountTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_account_account_type");
            });

            modelBuilder.Entity<AccountType>(entity =>
            {
                entity.ToTable("account_type");

                entity.Property(e => e.AccountTypeId)
                    .HasColumnName("account_type_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountTypeName)
                    .HasColumnName("account_type_name")
                    .HasMaxLength(500);

                entity.Property(e => e.BsLevel1)
                    .HasColumnName("bs_level1")
                    .HasMaxLength(50);

                entity.Property(e => e.BsLevel2)
                    .HasColumnName("bs_level2")
                    .HasMaxLength(50);

                entity.Property(e => e.BsLevel3)
                    .HasColumnName("bs_level3")
                    .HasMaxLength(50);

                entity.Property(e => e.BsLevel4)
                    .HasColumnName("bs_level4")
                    .HasMaxLength(50);

                entity.Property(e => e.CashFlow)
                    .HasColumnName("cash_flow")
                    .HasMaxLength(500);

                entity.Property(e => e.ConsolidatedAdjustedEbitda)
                    .HasColumnName("consolidated_adjusted_ebitda")
                    .HasMaxLength(500);

                entity.Property(e => e.PAndL)
                    .HasColumnName("p_and_l")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<AcibalanceSheet>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIBalanceSheet", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Acibudget>(entity =>
            {
                entity.ToTable("ACIBudget", "ACI");

                entity.Property(e => e.AcibudgetId).HasColumnName("ACIBudgetID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Timestatmp)
                    .HasColumnName("timestatmp")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Acicompany)
                    .WithMany(p => p.Acibudget)
                    .HasForeignKey(d => d.AcicompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ACIBudget_ACICompanyMaster");

                entity.HasOne(d => d.Acicustomer)
                    .WithMany(p => p.Acibudget)
                    .HasForeignKey(d => d.AcicustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ACIBudget_ACICustomerMaster");

                entity.HasOne(d => d.AcilineItem)
                    .WithMany(p => p.Acibudget)
                    .HasForeignKey(d => d.AcilineItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ACIBudget_ACIItemMaster");
            });

            modelBuilder.Entity<AcicompanyMaster>(entity =>
            {
                entity.HasKey(e => e.AcicompanyId);

                entity.ToTable("ACICompanyMaster", "ACI");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicompanyName)
                    .IsRequired()
                    .HasColumnName("ACICompanyName")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.QbcompanyId)
                    .HasColumnName("QBCompanyID")
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcicustomerMaster>(entity =>
            {
                entity.HasKey(e => e.AcicustomerId);

                entity.ToTable("ACICustomerMaster", "ACI");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerName)
                    .IsRequired()
                    .HasColumnName("ACICustomerName")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.QbcustomerId)
                    .HasColumnName("QBCustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Acidescretionary>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIDescretionary", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Acidistributor>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIDistributor", "ACI");

                entity.HasIndex(e => new { e.Aciamount, e.AcicompanyId, e.AcicustomerId, e.AcilineItemId, e.AcitransactionDate })
                    .HasName("nci_wi_ACIDistributor_D07388EC1C4EC6B6B8EDB9562138D72A");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.Aciqty).HasColumnName("ACIQty");

                entity.Property(e => e.Acirate).HasColumnName("ACIRate");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Acihr>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIHR", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AciindependentContractTask>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIIndependentContractTask", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AciitemMaster>(entity =>
            {
                entity.HasKey(e => e.AcilineItemId)
                    .HasName("PK_LineItemID");

                entity.ToTable("ACIItemMaster", "ACI");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AciitemCategory)
                    .IsRequired()
                    .HasColumnName("ACIItemCategory")
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.AciitemTypeId).HasColumnName("ACIItemTypeID");

                entity.Property(e => e.AcilineItemName)
                    .IsRequired()
                    .HasColumnName("ACILineItemName")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.QbaccountName)
                    .HasColumnName("QBAccountName")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.QbaccountNum)
                    .HasColumnName("QBAccountNum")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcikeyInfo>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIKeyInfo", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcimonthlyExpense>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIMonthlyExpense", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Aciprojection>(entity =>
            {
                entity.ToTable("ACIProjection", "ACI");

                entity.Property(e => e.AciprojectionId).HasColumnName("ACIProjectionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Timestatmp)
                    .HasColumnName("timestatmp")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Acicompany)
                    .WithMany(p => p.Aciprojection)
                    .HasForeignKey(d => d.AcicompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ACIProjection_ACICompanyMaster");

                entity.HasOne(d => d.AcilineItem)
                    .WithMany(p => p.Aciprojection)
                    .HasForeignKey(d => d.AcilineItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ACIProjection_ACIItemMaster");
            });

            modelBuilder.Entity<Acipublisher>(entity =>
            {
                entity.HasKey(e => e.AcitransactionId);

                entity.ToTable("ACIPublisher", "ACI");

                entity.Property(e => e.AcitransactionId).HasColumnName("ACITransactionID");

                entity.Property(e => e.Aciamount).HasColumnName("ACIAmount");

                entity.Property(e => e.AcicompanyId).HasColumnName("ACICompanyID");

                entity.Property(e => e.AcicustomerId).HasColumnName("ACICustomerID");

                entity.Property(e => e.AcilineItemId).HasColumnName("ACILineItemID");

                entity.Property(e => e.Aciqty).HasColumnName("ACIQty");

                entity.Property(e => e.Acirate).HasColumnName("ACIRate");

                entity.Property(e => e.AcitransactionDate)
                    .HasColumnName("ACITransactionDate")
                    .HasColumnType("date");

                entity.Property(e => e.Memo)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AgingDate>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("aging_date");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");
            });

            modelBuilder.Entity<BsDate>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("bs_date");

                entity.HasIndex(e => e.Date)
                    .HasName("index_date");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(150);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Budget>(entity =>
            {
                entity.ToTable("budget");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.BudgetAmount).HasColumnName("budget_amount");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Month)
                    .HasColumnName("month")
                    .HasColumnType("date");

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Budget)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_budget_account");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Budget)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_budget_customer");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Budget)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_budget_region");
            });

            modelBuilder.Entity<CashFlowProjection>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("cash_flow_projection");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .HasColumnType("date");

                entity.HasOne(d => d.Account)
                    .WithMany()
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_cash_flow_projection_account");

                entity.HasOne(d => d.Region)
                    .WithMany()
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_cash_flow_projection_region");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("customer_name")
                    .HasMaxLength(250);

                entity.Property(e => e.RegionId)
                    .IsRequired()
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Customer)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_customer_region");
            });

            modelBuilder.Entity<Ebitda>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("ebitda");

                entity.Property(e => e.AccountNum)
                    .HasColumnName("account_num")
                    .HasMaxLength(150);

                entity.Property(e => e.ConsolidatedAdjustedEbitda)
                    .HasColumnName("consolidated_adjusted_ebitda")
                    .HasMaxLength(500);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<IpaData>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("ipa_data");

                entity.Property(e => e.Account)
                    .HasColumnName("account")
                    .HasMaxLength(150);

                entity.Property(e => e.AccountType)
                    .HasColumnName("account_type")
                    .HasMaxLength(150);

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.Credit).HasColumnName("credit");

                entity.Property(e => e.CustomerName)
                    .HasColumnName("customer_name")
                    .HasMaxLength(150);

                entity.Property(e => e.Debit).HasColumnName("debit");

                entity.Property(e => e.Memo)
                    .HasColumnName("memo")
                    .HasMaxLength(150);

                entity.Property(e => e.Qty).HasColumnName("qty");

                entity.Property(e => e.SalesPrice).HasColumnName("sales_Price");

                entity.Property(e => e.Split)
                    .HasColumnName("split")
                    .HasMaxLength(150);

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .HasColumnType("date");

                entity.Property(e => e.TransactionNumber).HasColumnName("transaction_number");

                entity.Property(e => e.TransactionType)
                    .HasColumnName("transaction_type")
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Projection>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("projection");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .HasColumnType("date");

                entity.HasOne(d => d.Account)
                    .WithMany()
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_projection_account");

                entity.HasOne(d => d.Region)
                    .WithMany()
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_projection_region");
            });

            modelBuilder.Entity<ProjectionMonthly>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("projection_monthly");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .HasColumnType("date");

                entity.HasOne(d => d.Account)
                    .WithMany()
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_projection_monthly_account");

                entity.HasOne(d => d.Region)
                    .WithMany()
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_projection_monthly_region");
            });

            modelBuilder.Entity<QbDate>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("qb_date");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.Include)
                    .IsRequired()
                    .HasColumnName("include")
                    .HasMaxLength(50);

                entity.Property(e => e.Week)
                    .IsRequired()
                    .HasColumnName("week")
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("region");

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.RegionName)
                    .IsRequired()
                    .HasColumnName("region_name")
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transaction");

                entity.HasIndex(e => e.AccountId)
                    .HasName("index_transaction_account_id");

                entity.HasIndex(e => e.CustomerId)
                    .HasName("index_transaction_customer_id");

                entity.HasIndex(e => e.DueDate)
                    .HasName("index_due_date");

                entity.HasIndex(e => e.RegionId)
                    .HasName("index_transaction_region_id");

                entity.HasIndex(e => e.TransactionDate)
                    .HasName("index_transaction_date");

                entity.HasIndex(e => e.VendorId)
                    .HasName("index_transaction_vendor_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("date");

                entity.Property(e => e.DueDateH)
                    .HasColumnName("due_date_h")
                    .HasColumnType("date");

                entity.Property(e => e.IsPaid).HasColumnName("is_paid");

                entity.Property(e => e.IsPaidH).HasColumnName("is_paid_h");

                entity.Property(e => e.Memo)
                    .HasColumnName("memo")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.MemoH)
                    .HasColumnName("memo_h")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Qty).HasColumnName("qty");

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.SalesPrice).HasColumnName("sales_price");

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .HasColumnType("date");

                entity.Property(e => e.TransactionDateH)
                    .HasColumnName("transaction_date_h")
                    .HasColumnType("date");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VendorId)
                    .HasColumnName("vendor_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.VendorIdH)
                    .HasColumnName("vendor_id_h")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_transaction_account");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_transaction_customer");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_transaction_region");
            });

            modelBuilder.Entity<TransactionTmp>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("transaction_tmp");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("date");

                entity.Property(e => e.DueDateH)
                    .HasColumnName("due_date_h")
                    .HasColumnType("date");

                entity.Property(e => e.IsPaid).HasColumnName("is_paid");

                entity.Property(e => e.IsPaidH).HasColumnName("is_paid_h");

                entity.Property(e => e.Memo)
                    .HasColumnName("memo")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.MemoH)
                    .HasColumnName("memo_h")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Qty).HasColumnName("qty");

                entity.Property(e => e.RegionId)
                    .HasColumnName("region_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.SalesPrice).HasColumnName("sales_price");

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("transaction_date")
                    .HasColumnType("date");

                entity.Property(e => e.TransactionDateH)
                    .HasColumnName("transaction_date_h")
                    .HasColumnType("date");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VendorId)
                    .HasColumnName("vendor_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.VendorIdH)
                    .HasColumnName("vendor_id_h")
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.ToTable("vendor");

                entity.HasComment("vendor");

                entity.Property(e => e.VendorId)
                    .HasColumnName("vendor_id")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.VendorName)
                    .HasColumnName("vendor_name")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.VendorTypeName)
                    .HasColumnName("vendor_type_name")
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
